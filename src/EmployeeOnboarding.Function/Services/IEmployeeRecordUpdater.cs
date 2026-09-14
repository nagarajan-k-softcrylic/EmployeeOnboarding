namespace EmployeeOnboarding.Function.Services;

public interface IEmployeeRecordUpdater
{
    Task UpdateWelcomeLetterUrlAsync(int employeeId, string welcomeLetterUrl, CancellationToken ct = default);
}
