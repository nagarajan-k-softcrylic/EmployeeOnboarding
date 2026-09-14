using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace EmployeeOnboarding.Api.Infrastructure.Storage;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string containerName, string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);

        var blobClient = container.GetBlobClient(fileName);
        await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);

        _logger.LogInformation("Uploaded blob {FileName} to container {ContainerName}", fileName, containerName);
        return blobClient.Uri.ToString();
    }

    public async Task<(Stream Content, string ContentType)> DownloadFileAsync(string containerName, string fileName, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(ct))
        {
            throw new FileNotFoundException($"Blob '{fileName}' not found in container '{containerName}'.");
        }

        var download = await blobClient.DownloadContentAsync(ct);
        var stream = download.Value.Content.ToStream();
        var contentType = download.Value.Details.ContentType ?? "application/octet-stream";

        return (stream, contentType);
    }

    public async Task DeleteFileAsync(string containerName, string fileName, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        _logger.LogInformation("Deleted blob {FileName} from container {ContainerName}", fileName, containerName);
    }

    public Task<string> GenerateSasUrlAsync(string containerName, string fileName, TimeSpan expiry, CancellationToken ct = default)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient(fileName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Current credentials cannot generate SAS URI. Use a storage account key or user delegation key.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = fileName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return Task.FromResult(sasUri.ToString());
    }
}
