public interface IAzureStorageService
{
    Task<string> UploadImageAsync(IFormFile fileStream);
    Task DeleteImageAsync(string imageUrl);
    Task<string> GenerateSasTokenAsync(string imageUrl, TimeSpan expiryTime);
}