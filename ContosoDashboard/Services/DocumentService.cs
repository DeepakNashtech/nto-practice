using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".png", ".jpg", ".jpeg"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        INotificationService notificationService,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<Document>> GetUserDocumentsAsync(int userId, string? category = null, string? searchTerm = null)
    {
        var query = _context.Documents
            .Include(d => d.UploadedByUser)
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .Where(d => d.UploadedByUserId == userId || d.Shares.Any(s => s.SharedWithUserId == userId));

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(d => d.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(term) ||
                                     (d.Description != null && d.Description.ToLower().Contains(term)) ||
                                     (d.Tags != null && d.Tags.ToLower().Contains(term)) ||
                                     (d.UploadedByUser != null && d.UploadedByUser.DisplayName.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<List<Document>> GetProjectDocumentsAsync(int projectId)
    {
        return await _context.Documents
            .Include(d => d.UploadedByUser)
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId)
    {
        var doc = await _context.Documents
            .Include(d => d.UploadedByUser)
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (doc == null) return null;

        // Authorization check
        if (!await HasAccessAsync(doc, requestingUserId))
        {
            return null;
        }

        return doc;
    }

    public async Task<Document> UploadDocumentAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSize,
        string title,
        string category,
        string? description,
        int? projectId,
        string? tags,
        int userId)
    {
        // 1. Validation
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Document title is required.");
        }

        if (fileSize > MaxFileSizeBytes)
        {
            throw new ArgumentException($"File size ({fileSize / (1024 * 1024)} MB) exceeds the 25 MB limit.");
        }

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"File type '{ext}' is not supported. Allowed types: PDF, Word, Excel, PowerPoint, Text, PNG, JPEG.");
        }

        // 2. Physical File Storage (outside wwwroot)
        string relativeFilePath;
        try
        {
            relativeFilePath = await _fileStorageService.UploadAsync(fileStream, originalFileName, contentType, userId, projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist document to storage for user {UserId}", userId);
            throw new InvalidOperationException("Failed to save uploaded file to storage. " + ex.Message, ex);
        }

        // 3. Database Metadata Persistence
        var document = new Document
        {
            Title = title.Trim(),
            Description = description?.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? "Personal Files" : category.Trim(),
            FileName = Path.GetFileName(originalFileName),
            FilePath = relativeFilePath,
            FileSize = fileSize,
            FileType = contentType.Length > 255 ? contentType[..255] : contentType,
            ProjectId = projectId,
            UploadedByUserId = userId,
            UploadDate = DateTime.UtcNow,
            Tags = tags?.Trim()
        };

        try
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist document record to database, cleaning up disk file {FilePath}", relativeFilePath);
            await _fileStorageService.DeleteAsync(relativeFilePath);
            throw;
        }

        // 4. Notifications if associated with project
        if (projectId.HasValue)
        {
            try
            {
                var projectMembers = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == projectId.Value && pm.UserId != userId)
                    .Select(pm => pm.UserId)
                    .ToListAsync();

                foreach (var memberId in projectMembers)
                {
                    await _notificationService.CreateNotificationAsync(new Notification
                    {
                        UserId = memberId,
                        Title = "New Project Document",
                        Message = $"A new document '{document.Title}' was added to your project.",
                        Type = NotificationType.ProjectUpdate,
                        Priority = NotificationPriority.Informational
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not send project document notifications");
            }
        }

        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId)
    {
        var doc = await _context.Documents
            .Include(d => d.Project)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (doc == null) return false;

        var user = await _context.Users.FindAsync(requestingUserId);
        var isAdmin = user?.Role == UserRole.Administrator;
        var isPM = doc.Project != null && doc.Project.ProjectManagerId == requestingUserId;
        var isOwner = doc.UploadedByUserId == requestingUserId;

        if (!isOwner && !isPM && !isAdmin)
        {
            _logger.LogWarning("User {UserId} unauthorized to delete document {DocumentId}", requestingUserId, documentId);
            return false;
        }

        var filePath = doc.FilePath;

        // Remove from DB first
        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync();

        // Delete physical file
        try
        {
            await _fileStorageService.DeleteAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {FilePath} for document {DocumentId}", filePath, documentId);
        }

        return true;
    }

    public async Task<(Stream Stream, string FileName, string ContentType)> DownloadDocumentAsync(int documentId, int requestingUserId)
    {
        var doc = await _context.Documents
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (doc == null)
        {
            throw new FileNotFoundException("Document not found.");
        }

        if (!await HasAccessAsync(doc, requestingUserId))
        {
            throw new UnauthorizedAccessException("You do not have permission to download this document.");
        }

        var stream = await _fileStorageService.DownloadAsync(doc.FilePath);
        return (stream, doc.FileName, doc.FileType);
    }

    public async Task<bool> ShareDocumentAsync(int documentId, int targetUserId, int requestingUserId, string permission = "Read")
    {
        var doc = await _context.Documents.FindAsync(documentId);
        if (doc == null || doc.UploadedByUserId != requestingUserId)
        {
            return false;
        }

        var existing = await _context.DocumentShares
            .FirstOrDefaultAsync(s => s.DocumentId == documentId && s.SharedWithUserId == targetUserId);

        if (existing == null)
        {
            _context.DocumentShares.Add(new DocumentShare
            {
                DocumentId = documentId,
                SharedWithUserId = targetUserId,
                SharedByUserId = requestingUserId,
                SharedDate = DateTime.UtcNow,
                Permission = permission
            });
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(new Notification
            {
                UserId = targetUserId,
                Title = "Document Shared With You",
                Message = $"{doc.Title} has been shared with you.",
                Type = NotificationType.SystemAnnouncement,
                Priority = NotificationPriority.Informational
            });
        }

        return true;
    }

    public async Task<List<Document>> GetRecentDocumentsAsync(int userId, int count = 5)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId)
            .OrderByDescending(d => d.UploadDate)
            .Take(count)
            .ToListAsync();
    }

    private async Task<bool> HasAccessAsync(Document doc, int userId)
    {
        if (doc.UploadedByUserId == userId) return true;

        var user = await _context.Users.FindAsync(userId);
        if (user?.Role == UserRole.Administrator) return true;

        if (doc.ProjectId.HasValue)
        {
            var isMember = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == doc.ProjectId.Value && pm.UserId == userId);
            if (isMember) return true;
        }

        var isShared = await _context.DocumentShares
            .AnyAsync(s => s.DocumentId == doc.DocumentId && s.SharedWithUserId == userId);
        return isShared;
    }
}
