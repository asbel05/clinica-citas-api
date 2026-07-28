using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DoctorRepositoryImpl : IDoctorRepository
{
    private readonly AppDbContext _context;

    public DoctorRepositoryImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await _context.Doctores.AddAsync(doctor, cancellationToken);
    }

    public Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Doctores
            .FirstOrDefaultAsync(doctor => doctor.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Doctor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Doctores
            .AsNoTracking()
            .Where(doctor => doctor.Activo)
            .OrderBy(doctor => doctor.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> GetFutureScheduledAppointmentsAsync(int doctorId,
        DateTime from, CancellationToken cancellationToken = default)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(cita =>
                cita.DoctorId == doctorId &&
                cita.Estado == EstadoCita.Programada &&
                cita.FechaHoraInicio >= from)
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
