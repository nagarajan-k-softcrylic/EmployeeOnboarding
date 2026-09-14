using AutoMapper;
using EmployeeOnboarding.Api.Application.DTOs;
using EmployeeOnboarding.Api.Domain;
using EmployeeOnboarding.Api.Infrastructure.Messaging;
using EmployeeOnboarding.Api.Infrastructure.Storage;
using EmployeeOnboarding.Api.Infrastructure.UnitOfWork;
using MediatR;

namespace EmployeeOnboarding.Api.Application.Employees;

public class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesQuery, List<EmployeeResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllEmployeesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<EmployeeResponse>> Handle(GetAllEmployeesQuery request, CancellationToken ct)
    {
        var employees = await _uow.Employees.GetAllAsync(ct);
        return _mapper.Map<List<EmployeeResponse>>(employees);
    }
}

public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse?>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetEmployeeByIdHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse?> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.Id, ct);
        return employee is null ? null : _mapper.Map<EmployeeResponse>(employee);
    }
}

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEmployeeEventPublisher _publisher;
    private readonly ILogger<CreateEmployeeHandler> _logger;

    public CreateEmployeeHandler(IUnitOfWork uow, IMapper mapper, IEmployeeEventPublisher publisher, ILogger<CreateEmployeeHandler> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<EmployeeResponse> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        var employee = _mapper.Map<Employee>(request.Request);
        employee.CreatedDate = DateTime.UtcNow;

        await _uow.Employees.AddAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Employee {EmployeeCode} created with Id {EmployeeId}", employee.EmployeeCode, employee.Id);

        try
        {
            await _publisher.PublishEmployeeCreatedAsync(
                new EmployeeCreatedMessage(employee.Id, employee.EmployeeCode, employee.Name, employee.Email, employee.Department), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish EmployeeCreated event for EmployeeId {EmployeeId}", employee.Id);
        }

        return _mapper.Map<EmployeeResponse>(employee);
    }
}

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponse?>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateEmployeeHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse?> Handle(UpdateEmployeeCommand request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.Id, ct);
        if (employee is null) return null;

        _mapper.Map(request.Request, employee);
        employee.ModifiedDate = DateTime.UtcNow;

        _uow.Employees.Update(employee);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<EmployeeResponse>(employee);
    }
}

public class DeleteEmployeeHandler : IRequestHandler<DeleteEmployeeCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteEmployeeHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteEmployeeCommand request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.Id, ct);
        if (employee is null) return false;

        employee.IsDeleted = true;
        employee.ModifiedDate = DateTime.UtcNow;
        _uow.Employees.Delete(employee);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

public class UploadResumeHandler : IRequestHandler<UploadResumeCommand, string?>
{
    private readonly IUnitOfWork _uow;
    private readonly IBlobStorageService _blobStorageService;
    private const string ContainerName = "resumes";

    public UploadResumeHandler(IUnitOfWork uow, IBlobStorageService blobStorageService)
    {
        _uow = uow;
        _blobStorageService = blobStorageService;
    }

    public async Task<string?> Handle(UploadResumeCommand request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.Id, ct);
        if (employee is null) return null;

        var blobName = $"{employee.EmployeeCode}/{request.FileName}";
        var url = await _blobStorageService.UploadFileAsync(ContainerName, blobName, request.Content, request.ContentType, ct);

        employee.ResumeUrl = url;
        employee.ModifiedDate = DateTime.UtcNow;
        _uow.Employees.Update(employee);
        await _uow.SaveChangesAsync(ct);

        return url;
    }
}

public class DownloadResumeHandler : IRequestHandler<DownloadResumeQuery, (Stream Content, string ContentType, string FileName)?>
{
    private readonly IUnitOfWork _uow;
    private readonly IBlobStorageService _blobStorageService;
    private const string ContainerName = "resumes";

    public DownloadResumeHandler(IUnitOfWork uow, IBlobStorageService blobStorageService)
    {
        _uow = uow;
        _blobStorageService = blobStorageService;
    }

    public async Task<(Stream Content, string ContentType, string FileName)?> Handle(DownloadResumeQuery request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.Id, ct);
        if (employee is null || string.IsNullOrEmpty(employee.ResumeUrl)) return null;

        var fileName = employee.ResumeUrl.Split('/').Last();
        var blobName = $"{employee.EmployeeCode}/{fileName}";
        var (content, contentType) = await _blobStorageService.DownloadFileAsync(ContainerName, blobName, ct);

        return (content, contentType, fileName);
    }
}
