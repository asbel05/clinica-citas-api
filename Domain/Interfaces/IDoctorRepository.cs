using Domain.Entities;

namespace Domain.Interfaces;

public interface IDoctorRepository
{
    Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cita>> GetFutureScheduledAppointmentsAsync(int doctorId, 
        DateTime from, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
