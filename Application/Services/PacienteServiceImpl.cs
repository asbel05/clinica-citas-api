using Application.DTOs.Pacientes;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Services;

public class PacienteServiceImpl : IPacienteService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IValidator<CreatePacienteRequest> _validator;

    public PacienteServiceImpl(IPacienteRepository pacienteRepository, IValidator<CreatePacienteRequest> validator)
    {
        _pacienteRepository = pacienteRepository;
        _validator = validator;
    }

    public async Task<PacienteResponse> CreateAsync(CreatePacienteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        var documentExists = await _pacienteRepository
            .ExistsByDocumentAsync(request.DocumentoIdentidad, cancellationToken);

        if (documentExists)
        {
            throw new BusinessRuleException(
                "Ya existe un paciente registrado con ese documento de identidad.");
        }

        var paciente = PacienteMapper.ToEntity(request);

        await _pacienteRepository.AddAsync(paciente, cancellationToken);
        await _pacienteRepository.SaveChangesAsync(cancellationToken);

        return PacienteMapper.ToResponse(paciente);
    }

    public async Task<IReadOnlyList<PacienteResponse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var pacientes = await _pacienteRepository.GetActiveAsync(cancellationToken);

        return pacientes
            .Select(PacienteMapper.ToResponse)
            .ToList();
    }

    public async Task<PacienteResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var paciente = await GetRequiredAsync(id, cancellationToken);

        return PacienteMapper.ToResponse(paciente);
    }

    private async Task<Domain.Entities.Paciente> GetRequiredAsync(int id, CancellationToken cancellationToken)
    {
        var paciente = await _pacienteRepository.GetByIdAsync(id, cancellationToken);

        return paciente ?? throw new NotFoundException($"No se encontró el paciente con Id {id}.");
    }

    private async Task ValidateAsync(CreatePacienteRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
