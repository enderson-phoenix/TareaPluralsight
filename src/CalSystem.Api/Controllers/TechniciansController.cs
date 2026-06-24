using CalSystem.Application.Technicians.Commands.CreateTechnician;
using CalSystem.Application.Technicians.Queries.GetAllTechnicians;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CalSystem.Api.Controllers;

[ApiController]
[Route("api/technicians")]
public class TechniciansController : ControllerBase
{
    private readonly IMediator _mediator;

    public TechniciansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new technician.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTechnician([FromBody] CreateTechnicianRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Name and Email are required.");

        var command = new CreateTechnicianCommand(request.Name, request.Email);
        var id = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetAll), new { }, new { id });
    }

    /// <summary>Returns all registered technicians.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var technicians = await _mediator.Send(new GetAllTechniciansQuery());
        return Ok(technicians);
    }
}

public record CreateTechnicianRequest(string Name, string Email);
