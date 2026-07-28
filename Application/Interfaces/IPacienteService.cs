using Application.DTOs.Pacientes;

namespace Application.Interfaces;

public interface IPacienteService
{
    Task<PacienteResponse> CreateAsync(CreatePacienteRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PacienteResponse>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<PacienteResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
