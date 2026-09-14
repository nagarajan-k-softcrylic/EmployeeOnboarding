using EmployeeOnboarding.Function.Models;

namespace EmployeeOnboarding.Function.Services;

public interface IWelcomeLetterGenerator
{
    byte[] Generate(EmployeeCreatedMessage employee);
}
