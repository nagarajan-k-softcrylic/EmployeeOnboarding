namespace EmployeeOnboarding.Function.Models;

public record EmployeeCreatedMessage(int EmployeeId, string EmployeeCode, string Name, string Email, string Department);
