using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;

namespace Tests.Integration;

public class RepositoryIntegrationTests : SqliteTestBase
{
    [Fact]
    public async Task CitaRepository_ShouldPersistAndLoadRelatedData()
    {
        var doctor = new Doctor("Carlos Perez", "Cardiologia");
        var patient = new Paciente("Nelly Torres", "05372334");
        Context.Doctores.Add(doctor);
        Context.Pacientes.Add(patient);
        await Context.SaveChangesAsync();

        var start = DateTime.UtcNow.AddDays(2);
        var appointment = new Cita(doctor.Id, patient.Id, start, start.AddHours(1));
        var repository = new CitaRepositoryImpl(Context);

        await repository.AddAsync(appointment);
        await repository.SaveChangesAsync();

        var result = await repository.GetByIdAsync(appointment.Id);

        result.Should().NotBeNull();
        result!.Doctor.Should().NotBeNull();
        result.Doctor!.Nombre.Should().Be("Carlos Perez");
        result.Paciente.Should().NotBeNull();
        result.Paciente!.DocumentoIdentidad.Should().Be("05372334");
    }

    [Fact]
    public async Task CitaRepository_ShouldFilterByDoctorPatientAndStatus()
    {
        var doctor = new Doctor("Carlos Perez", "Cardiologia");
        var patient = new Paciente("Nelly Torres", "05372334");
        Context.Doctores.Add(doctor);
        Context.Pacientes.Add(patient);
        await Context.SaveChangesAsync();

        var scheduled = new Cita(
            doctor.Id,
            patient.Id,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1));
        var cancelled = new Cita(
            doctor.Id,
            patient.Id,
            DateTime.UtcNow.AddDays(3),
            DateTime.UtcNow.AddDays(3).AddHours(1));
        cancelled.Cancel("Cancelada para la prueba");
        Context.Citas.AddRange(scheduled, cancelled);
        await Context.SaveChangesAsync();

        var repository = new CitaRepositoryImpl(Context);

        var result = await repository.GetAsync(
            doctor.Id,
            patient.Id,
            EstadoCita.Programada);

        result.Should().ContainSingle();
        result[0].Estado.Should().Be(EstadoCita.Programada);
    }

    [Fact]
    public async Task CitaRepository_ShouldDetectOverlappingScheduledAppointments()
    {
        var doctor = new Doctor("Carlos Perez", "Cardiologia");
        var patient = new Paciente("Nelly Torres", "05372334");
        Context.Doctores.Add(doctor);
        Context.Pacientes.Add(patient);
        await Context.SaveChangesAsync();

        var start = DateTime.UtcNow.AddDays(2);
        Context.Citas.Add(new Cita(
            doctor.Id,
            patient.Id,
            start,
            start.AddHours(1)));
        await Context.SaveChangesAsync();

        var repository = new CitaRepositoryImpl(Context);
        var result = await repository.GetScheduledByDoctorAsync(
            doctor.Id,
            start.AddMinutes(30),
            start.AddHours(2));

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task PacienteRepository_ShouldDetectDuplicatedDocument()
    {
        Context.Pacientes.Add(new Paciente("Nelly Torres", "05372334"));
        await Context.SaveChangesAsync();

        var repository = new PacienteRepositoryImpl(Context);

        var exists = await repository.ExistsByDocumentAsync("05372334");

        exists.Should().BeTrue();
    }
}
