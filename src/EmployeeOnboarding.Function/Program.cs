using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
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
