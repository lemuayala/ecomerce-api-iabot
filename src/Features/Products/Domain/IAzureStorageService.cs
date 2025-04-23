public interface IAzureStorageService
{
    Task<string> UploadImageAsync(IFormFile fileStream, CancellationToken cancellationToken= default);
    Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken= default);
    Task<string> GenerateSasTokenAsync(string imageUrl, TimeSpan expiryTime);
}