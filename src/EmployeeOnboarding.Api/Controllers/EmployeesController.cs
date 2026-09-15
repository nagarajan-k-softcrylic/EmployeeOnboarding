using EmployeeOnboarding.Api.Application.DTOs;
using EmployeeOnboarding.Api.Application.Employees;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeOnboarding.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IMediator mediator, ILogger<EmployeesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmployeeResponse>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllEmployeesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateEmployeeCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Update(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateEmployeeCommand(id, request), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteEmployeeCommand(id), ct);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("upload-resume")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadResume([FromForm] int employeeId, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        await using var stream = file.OpenReadStream();
        var url = await _mediator.Send(new UploadResumeCommand(employeeId, file.FileName, stream, file.ContentType), ct);

        return url is null ? NotFound() : Ok(new { resumeUrl = url });
    }

    [HttpGet("download-resume/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadResume(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DownloadResumeQuery(id), ct);
        if (result is null) return NotFound();

        var (content, contentType, fileName) = result.Value;
        return File(content, contentType, fileName);
    }

    [HttpGet("download-welcome-letter/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadWelcomeLetter(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DownloadWelcomeLetterQuery(id), ct);
        if (result is null) return NotFound();

        var (content, contentType, fileName) = result.Value;
        return File(content, contentType, fileName);
    }
}
