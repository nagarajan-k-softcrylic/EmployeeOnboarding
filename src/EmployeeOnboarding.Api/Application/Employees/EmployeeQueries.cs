using EmployeeOnboarding.Api.Application.DTOs;
using MediatR;

namespace EmployeeOnboarding.Api.Application.Employees;

public record GetAllEmployeesQuery : IRequest<List<EmployeeResponse>>;

public record GetEmployeeByIdQuery(int Id) : IRequest<EmployeeResponse?>;

public record DownloadResumeQuery(int Id) : IRequest<(Stream Content, string ContentType, string FileName)?>;
