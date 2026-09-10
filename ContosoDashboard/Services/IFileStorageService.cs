namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, int userId, int? projectId = null);
    Task<Stream> DownloadAsync(string filePath);
    Task DeleteAsync(string filePath);
    Task<string> GetUrlAsync(string filePath, TimeSpan? expiration = null);
}
