using Application.DTOs.Citas;
using Application.Exceptions;
using Application.Services;
using Application.Validators.Citas;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Tests.Fakes;

namespace Tests.Services;

public class CitaServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateAppointmentWithReadableData()
    {
        var (service, _, _, _) = CreateService();
        var start = DateTime.UtcNow.AddDays(2);

        var response = await service.CreateAsync(new CreateCitaRequest
        {
            DoctorId = 1,
            PacienteId = 1,
            FechaHoraInicio = start,
            FechaHoraFin = start.AddHours(1)
        });

        response.DoctorNombre.Should().Be("Carlos Perez");
        response.DoctorEspecialidad.Should().Be("Cardiologia");
        response.PacienteNombre.Should().Be("Nelly Torres");
        response.PacienteDocumentoIdentidad.Should().Be("05372334");
        response.Estado.Should().Be(nameof(EstadoCita.Programada));
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectPastAppointment()
    {
        var (service, _, _, _) = CreateService();

        var action = () => service.CreateAsync(new CreateCitaRequest
        {
            DoctorId = 1,
            PacienteId = 1,
            FechaHoraInicio = DateTime.UtcNow.AddMinutes(-30),
            FechaHoraFin = DateTime.UtcNow.AddMinutes(-10)
        });

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*fecha futura*");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDoctorOverlap()
    {
        var (service, doctorRepository, _, citaRepository) = CreateService();
        var start = DateTime.UtcNow.AddDays(2);
        citaRepository.AddExisting(new Cita(1, 2, start, start.AddHours(1)));

        var action = () => service.CreateAsync(new CreateCitaRequest
        {
            DoctorId = 1,
            PacienteId = 1,
            FechaHoraInicio = start.AddMinutes(30),
            FechaHoraFin = start.AddHours(2)
        });

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*doctor ya tiene*horario*");
        doctorRepository.Doctors.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectPatientOverlap()
    {
        var (service, doctorRepository, _, citaRepository) = CreateService();
        var secondDoctor = new Doctor("Ana Ruiz", "Neurologia");
        doctorRepository.AddExisting(secondDoctor);
        var start = DateTime.UtcNow.AddDays(2);
        citaRepository.AddExisting(new Cita(1, 1, start, start.AddHours(1)));

        var action = () => service.CreateAsync(new CreateCitaRequest
        {
            DoctorId = secondDoctor.Id,
            PacienteId = 1,
            FechaHoraInicio = start.AddMinutes(30),
            FechaHoraFin = start.AddHours(2)
        });

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*paciente ya tiene*horario*");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMoreThanThreeScheduledAppointments()
    {
        var (service, _, _, citaRepository) = CreateService();

        for (var index = 1; index <= 3; index++)
        {
            var start = DateTime.UtcNow.AddDays(index + 1);
            citaRepository.AddExisting(new Cita(1, 1, start, start.AddHours(1)));
        }

        var action = () => service.CreateAsync(new CreateCitaRequest
        {
            DoctorId = 1,
            PacienteId = 1,
            FechaHoraInicio = DateTime.UtcNow.AddDays(10),
            FechaHoraFin = DateTime.UtcNow.AddDays(10).AddHours(1)
        });

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*3 citas programadas*");
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelFutureScheduledAppointment()
    {
        var (service, _, _, citaRepository) = CreateService();
        var cita = new Cita(
            1,
            1,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1));
        citaRepository.AddExisting(cita);

        await service.CancelAsync(cita.Id, new CancelCitaRequest
        {
            MotivoCancelacion = "Cambio de horario"
        });

        cita.Estado.Should().Be(EstadoCita.Cancelada);
        cita.MotivoCancelacion.Should().Be("Cambio de horario");
    }

    [Fact]
    public async Task CancelAsync_ShouldRejectAlreadyCancelledAppointment()
    {
        var (service, _, _, citaRepository) = CreateService();
        var cita = new Cita(
            1,
            1,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1));
        cita.Cancel("Ya cancelada");
        citaRepository.AddExisting(cita);

        var action = () => service.CancelAsync(cita.Id, new CancelCitaRequest());

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*programadas*");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidTimeRange()
    {
        var (service, _, _, _) = CreateService();
        var start = DateTime.UtcNow.AddDays(2);

        var action = () => service.CreateAsync(new CreateCitaRequest
        {
            DoctorId = 1,
            PacienteId = 1,
            FechaHoraInicio = start,
            FechaHoraFin = start.AddMinutes(-1)
        });

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    private static (
        CitaServiceImpl Service,
        FakeDoctorRepository Doctors,
        FakePacienteRepository Patients,
        FakeCitaRepository Appointments) CreateService()
    {
        var doctorRepository = new FakeDoctorRepository();
        doctorRepository.AddExisting(new Doctor("Carlos Perez", "Cardiologia"));

        var patientRepository = new FakePacienteRepository();
        patientRepository.AddExisting(new Paciente("Nelly Torres", "05372334"));

        var citaRepository = new FakeCitaRepository();
        var service = new CitaServiceImpl(
            citaRepository,
            doctorRepository,
            patientRepository,
            new CreateCitaValidator(),
            new CancelCitaValidator());

        return (service, doctorRepository, patientRepository, citaRepository);
    }
}
