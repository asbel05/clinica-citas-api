using Application.DTOs.Doctores;

namespace Application.Interfaces;

public interface IDoctorService
{
    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorResponse>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
