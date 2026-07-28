using Domain.Entities;

namespace Domain.Interfaces;

public interface IPacienteRepository
{
    Task AddAsync(Paciente paciente, CancellationToken cancellationToken = default);

    Task<Paciente?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Paciente>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByDocumentAsync(string documentoIdentidad, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
