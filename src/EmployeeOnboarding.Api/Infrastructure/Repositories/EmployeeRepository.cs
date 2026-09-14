using EmployeeOnboarding.Api.Domain;
using EmployeeOnboarding.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeOnboarding.Api.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Employees.AsNoTracking().OrderByDescending(e => e.CreatedDate).ToListAsync(ct);

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Employee employee, CancellationToken ct = default) =>
        await _context.Employees.AddAsync(employee, ct);

    public void Update(Employee employee) => _context.Employees.Update(employee);

    public void Delete(Employee employee) => _context.Employees.Update(employee);
}
