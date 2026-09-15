using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EmployeeOnboarding.Function.Models;
using EmployeeOnboarding.Function.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmployeeOnboarding.Function.Functions;

public class EmployeeCreatedFunction
{
    private const string ContainerName = "welcome-letters";

    private readonly ILogger<EmployeeCreatedFunction> _logger;
    private readonly IWelcomeLetterGenerator _letterGenerator;
    private readonly IEmployeeRecordUpdater _employeeRecordUpdater;
    private readonly BlobServiceClient _blobServiceClient;

    public EmployeeCreatedFunction(
        ILogger<EmployeeCreatedFunction> logger,
        IWelcomeLetterGenerator letterGenerator,
        IEmployeeRecordUpdater employeeRecordUpdater,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _letterGenerator = letterGenerator;
        _employeeRecordUpdater = employeeRecordUpdater;
        _blobServiceClient = blobServiceClient;
    }

    [Function("EmployeeCreatedFunction")]
    public async Task Run(
        [ServiceBusTrigger("employee-onboarding-queue", Connection = "ServiceBusConnectionString")] string messageBody,
        FunctionContext context)
    {
        var startedAt = DateTime.UtcNow;
        _logger.LogInformation("EmployeeCreatedFunction triggered. Payload: {Payload}", messageBody);

        try
        {
            // 1. Receive employee-onboarding-queue message
            var employee = JsonSerializer.Deserialize<EmployeeCreatedMessage>(messageBody)
                ?? throw new InvalidOperationException("Unable to deserialize EmployeeCreatedMessage.");

            // 2. Generate Welcome Letter PDF
            var pdfBytes = _letterGenerator.Generate(employee);

            // 3. Upload PDF to Blob Storage
            var container = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobName = $"{employee.EmployeeCode}/WelcomeLetter_{employee.EmployeeCode}.pdf";
            var blobClient = container.GetBlobClient(blobName);

            using (var stream = new MemoryStream(pdfBytes))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = "application/pdf" });

            // 4. Update Employee Record
            await _employeeRecordUpdater.UpdateWelcomeLetterUrlAsync(employee.EmployeeId, blobClient.Uri.ToString());

            // 5. Log telemetry
            var duration = DateTime.UtcNow - startedAt;
            _logger.LogInformation(
                "Welcome letter generated for EmployeeId {EmployeeId} ({EmployeeCode}) in {DurationMs}ms. Blob: {BlobUrl}",
                employee.EmployeeId, employee.EmployeeCode, duration.TotalMilliseconds, blobClient.Uri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process EmployeeCreated message: {Payload}", messageBody);
            throw; // Let the Functions runtime handle retry/dead-lettering
        }
    }
}
