using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface ICitaRepository
{
    Task AddAsync(Cita cita, CancellationToken cancellationToken = default);

    Task<Cita?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cita>> GetAsync(int? doctorId = null, int? pacienteId = null,
        EstadoCita? estado = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cita>> GetScheduledByDoctorAsync(int doctorId, DateTime from, 
        DateTime to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cita>> GetScheduledByPatientAsync(int pacienteId, DateTime from, 
        DateTime to, CancellationToken cancellationToken = default);

    Task<int> CountScheduledByPatientAsync(int pacienteId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
