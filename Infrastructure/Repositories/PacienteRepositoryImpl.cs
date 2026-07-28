using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PacienteRepositoryImpl : IPacienteRepository
{
    private readonly AppDbContext _context;

    public PacienteRepositoryImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default)
    {
        await _context.Pacientes.AddAsync(paciente, cancellationToken);
    }

    public Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Pacientes
            .FirstOrDefaultAsync(paciente => paciente.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Paciente>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .Where(paciente => paciente.Activo)
            .OrderBy(paciente => paciente.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByDocumentAsync(string documentoIdentidad, CancellationToken cancellationToken = default)
    {
        return _context.Pacientes
            .AnyAsync(
                paciente => paciente.DocumentoIdentidad == documentoIdentidad,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
