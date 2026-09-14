using EmployeeOnboarding.Api.Application.DTOs;
using MediatR;

namespace EmployeeOnboarding.Api.Application.Employees;

public record CreateEmployeeCommand(CreateEmployeeRequest Request) : IRequest<EmployeeResponse>;

public record UpdateEmployeeCommand(int Id, UpdateEmployeeRequest Request) : IRequest<EmployeeResponse?>;

public record DeleteEmployeeCommand(int Id) : IRequest<bool>;

public record UploadResumeCommand(int Id, string FileName, Stream Content, string ContentType) : IRequest<string?>;
