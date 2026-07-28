using Application.DTOs.Citas;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappers;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentValidation;

namespace Application.Services;

public class CitaServiceImpl : ICitaService
{
    private const int MaxScheduledAppointmentsPerPatient = 3;

    private readonly ICitaRepository _citaRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IValidator<CreateCitaRequest> _createValidator;
    private readonly IValidator<CancelCitaRequest> _cancelValidator;

    public CitaServiceImpl(ICitaRepository citaRepository, IDoctorRepository doctorRepository,
        IPacienteRepository pacienteRepository, IValidator<CreateCitaRequest> createValidator, 
        IValidator<CancelCitaRequest> cancelValidator)
    {
        _citaRepository = citaRepository;
        _doctorRepository = doctorRepository;
        _pacienteRepository = pacienteRepository;
        _createValidator = createValidator;
        _cancelValidator = cancelValidator;
    }

    public async Task<CitaResponse> CreateAsync(CreateCitaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        var now = DateTime.UtcNow;

        if (request.FechaHoraInicio <= now)
        {
            throw new BusinessRuleException("La cita debe programarse para una fecha futura.");
        }

        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);

        if (doctor is null)
        {
            throw new NotFoundException($"No se encontró el doctor con Id {request.DoctorId}.");
        }

        if (!doctor.Activo)
        {
            throw new BusinessRuleException("No se puede crear una cita con un doctor inactivo.");
        }

        var paciente = await _pacienteRepository.GetByIdAsync(request.PacienteId, cancellationToken);

        if (paciente is null)
        {
            throw new NotFoundException($"No se encontró el paciente con Id {request.PacienteId}.");
        }

        if (!paciente.Activo)
        {
            throw new BusinessRuleException("No se puede crear una cita con un paciente inactivo.");
        }

        var doctorHasOverlap = await _citaRepository.GetScheduledByDoctorAsync(
            request.DoctorId,
            request.FechaHoraInicio,
            request.FechaHoraFin,
            cancellationToken);

        if (doctorHasOverlap.Count > 0)
        {
            throw new BusinessRuleException(
                "El doctor ya tiene una cita programada en ese horario.");
        }

        var patientHasOverlap = await _citaRepository.GetScheduledByPatientAsync(
            request.PacienteId,
            request.FechaHoraInicio,
            request.FechaHoraFin,
            cancellationToken);

        if (patientHasOverlap.Count > 0)
        {
            throw new BusinessRuleException(
                "El paciente ya tiene una cita programada en ese horario.");
        }

        var scheduledAppointments = await _citaRepository
            .CountScheduledByPatientAsync(request.PacienteId, cancellationToken);

        if (scheduledAppointments >= MaxScheduledAppointmentsPerPatient)
        {
            throw new BusinessRuleException(
                "El paciente no puede tener más de 3 citas programadas.");
        }

        var cita = CitaMapper.ToEntity(request);

        await _citaRepository.AddAsync(cita, cancellationToken);
        await _citaRepository.SaveChangesAsync(cancellationToken);

        return CitaMapper.ToResponse(cita);
    }

    public async Task<IReadOnlyList<CitaResponse>> GetAsync(int? doctorId = null, int? pacienteId = null,
        EstadoCita? estado = null, CancellationToken cancellationToken = default)
    {
        var citas = await _citaRepository.GetAsync(
            doctorId,
            pacienteId,
            estado,
            cancellationToken);

        return citas
            .Select(CitaMapper.ToResponse)
            .ToList();
    }

    public async Task<CitaResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cita = await GetRequiredAsync(id, cancellationToken);

        return CitaMapper.ToResponse(cita);
    }

    public async Task CancelAsync(int id, CancelCitaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_cancelValidator, request, cancellationToken);

        var cita = await GetRequiredAsync(id, cancellationToken);

        if (cita.Estado != EstadoCita.Programada)
        {
            throw new BusinessRuleException(
                "Solo se pueden cancelar citas que se encuentren programadas.");
        }

        if (cita.FechaHoraInicio <= DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "No se puede cancelar una cita cuyo horario ya comenzó.");
        }

        cita.Cancel(request.MotivoCancelacion?.Trim());
        await _citaRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cita> GetRequiredAsync(int id, CancellationToken cancellationToken)
    {
        var cita = await _citaRepository.GetByIdAsync(id, cancellationToken);

        return cita ?? throw new NotFoundException($"No se encontró la cita con Id {id}.");
    }

    private static async Task ValidateAsync<TRequest>(IValidator<TRequest> validator,
        TRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
