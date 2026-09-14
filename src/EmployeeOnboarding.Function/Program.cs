using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
    {
        // ---- Azure Key Vault (Managed Identity in Azure, DefaultAzureCredential locally via az login) ----
        var builtConfig = configBuilder.Build();
        var keyVaultUri = builtConfig["KeyVaultUri"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri) && Uri.TryCreate(keyVaultUri, UriKind.Absolute, out var vaultUri))
        {
            configBuilder.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
        }
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Application Insights telemetry is wired automatically via the
        // APPLICATIONINSIGHTS_CONNECTION_STRING app setting on Azure Functions runtime.
        services.AddSingleton(_ =>
            new BlobServiceClient(hostContext.Configuration["StorageConnectionString"]));

        services.AddScoped<EmployeeOnboarding.Function.Services.IWelcomeLetterGenerator,
            EmployeeOnboarding.Function.Services.WelcomeLetterGenerator>();
        services.AddScoped<EmployeeOnboarding.Function.Services.IEmployeeRecordUpdater,
            EmployeeOnboarding.Function.Services.EmployeeRecordUpdater>();
    })
    .Build();

host.Run();
