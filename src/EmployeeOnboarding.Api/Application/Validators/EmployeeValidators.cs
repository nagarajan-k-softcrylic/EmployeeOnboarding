using EmployeeOnboarding.Api.Application.DTOs;
using FluentValidation;

namespace EmployeeOnboarding.Api.Application.Validators;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfJoining).NotEmpty();
    }
}

public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfJoining).NotEmpty();
    }
}
