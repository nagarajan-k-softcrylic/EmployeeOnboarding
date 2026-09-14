using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using EmployeeOnboarding.Api.Infrastructure.Messaging;
using EmployeeOnboarding.Api.Infrastructure.Persistence;
using EmployeeOnboarding.Api.Infrastructure.Repositories;
using EmployeeOnboarding.Api.Infrastructure.Storage;
using EmployeeOnboarding.Api.Infrastructure.UnitOfWork;
using EmployeeOnboarding.Api.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Azure Key Vault (Managed Identity in Azure, DefaultAzureCredential locally via az login) ----
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri) && Uri.TryCreate(keyVaultUri, UriKind.Absolute, out var vaultUri))
{
    builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
}

// ---- Serilog ----
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ---- Application Insights (only if a connection string is configured) ----
var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// ---- Controllers, Swagger, Validation ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Employee Onboarding API", Version = "v1" });
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ---- EF Core / Azure SQL ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString")));

// ---- Repository / UnitOfWork ----
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---- AutoMapper ----
builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

// ---- MediatR ----
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// ---- Azure Blob Storage ----
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration["StorageConnectionString"];
    return new BlobServiceClient(connectionString);
});
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// ---- Azure Service Bus ----
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration["ServiceBusConnectionString"];
    return new ServiceBusClient(connectionString);
});
builder.Services.AddScoped<IEmployeeEventPublisher, ServiceBusEmployeeEventPublisher>();

// ---- CORS for Angular dev server ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Onboarding API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
