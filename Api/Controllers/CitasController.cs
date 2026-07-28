using Application.DTOs.Citas;
using Application.Interfaces;
using Api.Errors;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/citas")]
[Produces("application/json")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;

    public CitasController(ICitaService citaService)
    {
        _citaService = citaService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CitaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CitaResponse>> Create(
        [FromBody] CreateCitaRequest request, CancellationToken cancellationToken)
    {
        var response = await _citaService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CitaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CitaResponse>>> Get([FromQuery] int? doctorId, [FromQuery] int? pacienteId,
        [FromQuery] EstadoCita? estado, CancellationToken cancellationToken)
    {
        var response = await _citaService.GetAsync(
            doctorId,
            pacienteId,
            estado,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CitaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CitaResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _citaService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelCitaRequest request, CancellationToken cancellationToken)
    {
        await _citaService.CancelAsync(id, request, cancellationToken);

        return NoContent();
    }
}
