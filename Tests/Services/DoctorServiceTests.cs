using Application.DTOs.Doctores;
using Application.Exceptions;
using Application.Services;
using Application.Validators.Doctores;
using Domain.Entities;
using FluentAssertions;
using Tests.Fakes;

namespace Tests.Services;

public class DoctorServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateDoctor()
    {
        var repository = new FakeDoctorRepository();
        var service = new DoctorServiceImpl(repository, new CreateDoctorValidator());

        var response = await service.CreateAsync(new CreateDoctorRequest
        {
            Nombre = "Carlos Perez",
            Especialidad = "Cardiologia"
        });

        response.Nombre.Should().Be("Carlos Perez");
        response.Especialidad.Should().Be("Cardiologia");
        response.Activo.Should().BeTrue();
        repository.Doctors.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidRequest()
    {
        var service = new DoctorServiceImpl(
            new FakeDoctorRepository(),
            new CreateDoctorValidator());

        var action = () => service.CreateAsync(new CreateDoctorRequest
        {
            Nombre = string.Empty,
            Especialidad = string.Empty
        });

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task DeactivateAsync_ShouldRejectDoctorWithFutureAppointments()
    {
        var doctorRepository = new FakeDoctorRepository();
        var doctor = new Doctor("Carlos Perez", "Cardiologia");
        doctorRepository.AddExisting(doctor);
        doctorRepository.Appointments.Add(new Cita(
            doctor.Id,
            1,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1)));

        var service = new DoctorServiceImpl(
            doctorRepository,
            new CreateDoctorValidator());

        var action = () => service.DeactivateAsync(doctor.Id);

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*citas futuras*");
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateDoctorWithoutFutureAppointments()
    {
        var repository = new FakeDoctorRepository();
        var doctor = new Doctor("Carlos Perez", "Cardiologia");
        repository.AddExisting(doctor);
        var service = new DoctorServiceImpl(repository, new CreateDoctorValidator());

        await service.DeactivateAsync(doctor.Id);

        doctor.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowWhenDoctorDoesNotExist()
    {
        var service = new DoctorServiceImpl(
            new FakeDoctorRepository(),
            new CreateDoctorValidator());

        var action = () => service.GetByIdAsync(99);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
