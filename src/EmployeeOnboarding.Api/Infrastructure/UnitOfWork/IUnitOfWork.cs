using EmployeeOnboarding.Api.Infrastructure.Repositories;

namespace EmployeeOnboarding.Api.Infrastructure.UnitOfWork;

public interface IUnitOfWork
{
    IEmployeeRepository Employees { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
