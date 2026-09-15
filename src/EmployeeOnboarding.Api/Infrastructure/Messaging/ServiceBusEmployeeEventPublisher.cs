using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Polly;
using Polly.Retry;

namespace EmployeeOnboarding.Api.Infrastructure.Messaging;

public class ServiceBusEmployeeEventPublisher : IEmployeeEventPublisher, IAsyncDisposable
{
    private const string QueueName = "employee-onboarding-queue";
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusEmployeeEventPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ServiceBusEmployeeEventPublisher(ServiceBusClient client, ILogger<ServiceBusEmployeeEventPublisher> logger)
    {
        _client = client;
        _sender = _client.CreateSender(QueueName);
        _logger = logger;

        _retryPolicy = Policy
            .Handle<ServiceBusException>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (ex, delay, attempt, _) =>
                    _logger.LogWarning(ex, "Retry {Attempt} publishing to {Queue} after {Delay}s", attempt, QueueName, delay.TotalSeconds));
    }

    public async Task PublishEmployeeCreatedAsync(EmployeeCreatedMessage message, CancellationToken ct = default)
    {
        var body = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = "EmployeeCreated"
        };

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _sender.SendMessageAsync(serviceBusMessage, ct);
            _logger.LogInformation("Published EmployeeCreated message for EmployeeId {EmployeeId}", message.EmployeeId);
        });
    }

    public async ValueTask DisposeAsync()
    {
        // Only dispose the sender created by this (scoped) instance.
        // _client is the app-wide Singleton ServiceBusClient and must NOT be disposed here,
        // otherwise it gets closed after the first request and breaks all subsequent publishes.
        await _sender.DisposeAsync();
    }
}
