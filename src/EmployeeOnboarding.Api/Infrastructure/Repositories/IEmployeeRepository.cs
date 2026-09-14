using EmployeeOnboarding.Api.Domain;

namespace EmployeeOnboarding.Api.Infrastructure.Repositories;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    void Update(Employee employee);
    void Delete(Employee employee);
}
