using AutoMapper;
using EmployeeOnboarding.Api.Application.DTOs;
using EmployeeOnboarding.Api.Domain;

namespace EmployeeOnboarding.Api.Application.Mapping;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeResponse>();
        CreateMap<CreateEmployeeRequest, Employee>();
        CreateMap<UpdateEmployeeRequest, Employee>();
    }
}
