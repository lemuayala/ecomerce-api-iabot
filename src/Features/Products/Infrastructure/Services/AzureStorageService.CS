using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

public class AzureStorageService : IAzureStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureStorageService(BlobServiceClient blobServiceClient)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(
        Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONTAINER"));

        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = _containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = GetContentType(file.FileName),
                CacheControl = "public, max-age=31536000"
            }
        }, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default) // Añadir CancellationToken
    {
        // Validar la URL antes de usarla
        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            var blobName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrEmpty(blobName))
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, cancellationToken);
            }
        }
    }

    public async Task<string> GenerateSasTokenAsync(string imageUrl, TimeSpan expiryTime)
    {
        var blobName = Path.GetFileName(new Uri(imageUrl).LocalPath);
        var blobClient = _containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiryTime)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = blobClient.GenerateSasUri(sasBuilder);
        return sasToken.ToString();
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}