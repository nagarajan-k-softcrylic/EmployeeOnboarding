namespace EmployeeOnboarding.Api.Infrastructure.Messaging;

public record EmployeeCreatedMessage(int EmployeeId, string EmployeeCode, string Name, string Email, string Department);

public interface IEmployeeEventPublisher
{
    Task PublishEmployeeCreatedAsync(EmployeeCreatedMessage message, CancellationToken ct = default);
}
