using Application.DTOs.Doctores;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Services;

public class DoctorServiceImpl : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IValidator<CreateDoctorRequest> _validator;

    public DoctorServiceImpl(IDoctorRepository doctorRepository, IValidator<CreateDoctorRequest> validator)
    {
        _doctorRepository = doctorRepository;
        _validator = validator;
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(request, cancellationToken);

        var doctor = DoctorMapper.ToEntity(request);

        await _doctorRepository.AddAsync(doctor, cancellationToken);
        await _doctorRepository.SaveChangesAsync(cancellationToken);

        return DoctorMapper.ToResponse(doctor);
    }

    public async Task<IReadOnlyList<DoctorResponse>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var doctors = await _doctorRepository.GetActiveAsync(cancellationToken);

        return doctors
            .Select(DoctorMapper.ToResponse)
            .ToList();
    }

    public async Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await GetRequiredAsync(id, cancellationToken);

        return DoctorMapper.ToResponse(doctor);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await GetRequiredAsync(id, cancellationToken);

        if (!doctor.Activo)
        {
            throw new BusinessRuleException("El doctor ya se encuentra inactivo.");
        }

        var futureAppointments = await _doctorRepository
            .GetFutureScheduledAppointmentsAsync(id, DateTime.UtcNow, cancellationToken);

        if (futureAppointments.Count > 0)
        {
            throw new BusinessRuleException(
                "No se puede desactivar el doctor porque tiene citas futuras programadas.");
        }

        doctor.Deactivate();
        await _doctorRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.Doctor> GetRequiredAsync(int id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(id, cancellationToken);

        return doctor ?? throw new NotFoundException($"No se encontró el doctor con Id {id}.");
    }

    private async Task ValidateAsync(CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
