using EmployeeOnboarding.Api.Infrastructure.Persistence;
using EmployeeOnboarding.Api.Infrastructure.Repositories;

namespace EmployeeOnboarding.Api.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public IEmployeeRepository Employees { get; }

    public UnitOfWork(AppDbContext context, IEmployeeRepository employeeRepository)
    {
        _context = context;
        Employees = employeeRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
