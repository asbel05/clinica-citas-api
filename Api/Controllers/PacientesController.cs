using Application.DTOs.Pacientes;
using Application.Interfaces;
using Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/pacientes")]
[Produces("application/json")]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;

    public PacientesController(IPacienteService pacienteService)
    {
        _pacienteService = pacienteService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PacienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PacienteResponse>> Create(
        [FromBody] CreatePacienteRequest request, CancellationToken cancellationToken)
    {
        var response = await _pacienteService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PacienteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PacienteResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var response = await _pacienteService.GetActiveAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PacienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PacienteResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _pacienteService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }
}
