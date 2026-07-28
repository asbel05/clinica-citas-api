using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CitaRepositoryImpl : ICitaRepository
{
    private readonly AppDbContext _context;

    public CitaRepositoryImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Cita cita, CancellationToken cancellationToken = default)
    {
        await _context.Citas.AddAsync(cita, cancellationToken);
    }

    public Task<Cita?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Citas
            .Include(cita => cita.Doctor)
            .Include(cita => cita.Paciente)
            .FirstOrDefaultAsync(cita => cita.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> GetAsync(int? doctorId = null, int? pacienteId = null,
        EstadoCita? estado = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Citas
            .AsNoTracking()
            .Include(cita => cita.Doctor)
            .Include(cita => cita.Paciente)
            .AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(cita => cita.DoctorId == doctorId.Value);
        }

        if (pacienteId.HasValue)
        {
            query = query.Where(cita => cita.PacienteId == pacienteId.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(cita => cita.Estado == estado.Value);
        }

        return await query
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> GetScheduledByDoctorAsync(int doctorId,
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(cita =>
                cita.DoctorId == doctorId &&
                cita.Estado == EstadoCita.Programada &&
                cita.FechaHoraInicio < to &&
                cita.FechaHoraFin > from)
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cita>> GetScheduledByPatientAsync(int pacienteId,
        DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(cita =>
                cita.PacienteId == pacienteId &&
                cita.Estado == EstadoCita.Programada &&
                cita.FechaHoraInicio < to &&
                cita.FechaHoraFin > from)
            .OrderBy(cita => cita.FechaHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountScheduledByPatientAsync(int pacienteId, CancellationToken cancellationToken = default)
    {
        return _context.Citas
            .CountAsync(
                cita =>
                    cita.PacienteId == pacienteId &&
                    cita.Estado == EstadoCita.Programada,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
