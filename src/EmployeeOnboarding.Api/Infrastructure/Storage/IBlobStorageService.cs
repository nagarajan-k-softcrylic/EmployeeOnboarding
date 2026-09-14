namespace EmployeeOnboarding.Api.Infrastructure.Storage;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(string containerName, string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task<(Stream Content, string ContentType)> DownloadFileAsync(string containerName, string fileName, CancellationToken ct = default);
    Task DeleteFileAsync(string containerName, string fileName, CancellationToken ct = default);
    Task<string> GenerateSasUrlAsync(string containerName, string fileName, TimeSpan expiry, CancellationToken ct = default);
}
