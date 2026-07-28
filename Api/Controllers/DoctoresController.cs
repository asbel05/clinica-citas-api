using Application.DTOs.Doctores;
using Application.Interfaces;
using Api.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/doctores")]
[Produces("application/json")]
public class DoctoresController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctoresController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorResponse>> Create(
        [FromBody] CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var response = await _doctorService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var response = await _doctorService.GetActiveAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _doctorService.GetByIdAsync(id, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:int}/desactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _doctorService.DeactivateAsync(id, cancellationToken);

        return NoContent();
    }
}
