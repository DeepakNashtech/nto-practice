using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<List<Document>> GetUserDocumentsAsync(int userId, string? category = null, string? searchTerm = null);
    Task<List<Document>> GetProjectDocumentsAsync(int projectId);
    Task<Document?> GetDocumentByIdAsync(int documentId, int requestingUserId);
    Task<Document> UploadDocumentAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSize,
        string title,
        string category,
        string? description,
        int? projectId,
        string? tags,
        int userId);
    Task<bool> DeleteDocumentAsync(int documentId, int requestingUserId);
    Task<(Stream Stream, string FileName, string ContentType)> DownloadDocumentAsync(int documentId, int requestingUserId);
    Task<bool> ShareDocumentAsync(int documentId, int targetUserId, int requestingUserId, string permission = "Read");
    Task<List<Document>> GetRecentDocumentsAsync(int userId, int count = 5);
}
