namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        // Protected directory outside wwwroot
        _baseStoragePath = Path.Combine(env.ContentRootPath, "AppData", "uploads");
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, int userId, int? projectId = null)
    {
        var projectSegment = projectId.HasValue ? $"project-{projectId.Value}" : "personal";
        var userFolder = Path.Combine(_baseStoragePath, userId.ToString(), projectSegment);
        
        if (!Directory.Exists(userFolder))
        {
            Directory.CreateDirectory(userFolder);
        }

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(userFolder, uniqueFileName);

        using (var outputStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(outputStream);
        }

        // Return relative path portable across machines
        var relativePath = Path.Combine("AppData", "uploads", userId.ToString(), projectSegment, uniqueFileName).Replace('\\', '/');
        _logger.LogInformation("File saved locally to {PhysicalPath} with relative path {RelativePath}", physicalPath, relativePath);
        return relativePath;
    }

    public Task<Stream> DownloadAsync(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            // Try relative to base content root
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Document file not found at {filePath}");
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file at {FullPath}", fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string filePath, TimeSpan? expiration = null)
    {
        // For local storage, provide relative download endpoint route
        return Task.FromResult($"/api/documents/download?path={Uri.EscapeDataString(filePath)}");
    }
}
