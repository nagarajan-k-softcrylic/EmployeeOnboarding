using EmployeeOnboarding.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeOnboarding.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);
            entity.Property(e => e.WelcomeLetterUrl).HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        base.OnModelCreating(modelBuilder);
    }
}
