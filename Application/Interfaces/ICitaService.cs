using Application.DTOs.Citas;
using Domain.Enums;

namespace Application.Interfaces;

public interface ICitaService
{
    Task<CitaResponse> CreateAsync(CreateCitaRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CitaResponse>> GetAsync(int? doctorId = null, int? pacienteId = null, 
        EstadoCita? estado = null, CancellationToken cancellationToken = default);

    Task<CitaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CancelAsync(int id, CancelCitaRequest request, CancellationToken cancellationToken = default);
}
