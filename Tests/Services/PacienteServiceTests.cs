using Application.DTOs.Pacientes;
using Application.Exceptions;
using Application.Services;
using Application.Validators.Pacientes;
using Domain.Entities;
using FluentAssertions;
using Tests.Fakes;

namespace Tests.Services;

public class PacienteServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreatePatient()
    {
        var repository = new FakePacienteRepository();
        var service = new PacienteServiceImpl(repository, new CreatePacienteValidator());

        var response = await service.CreateAsync(new CreatePacienteRequest
        {
            Nombre = "Nelly Torres",
            DocumentoIdentidad = "05372334"
        });

        response.Nombre.Should().Be("Nelly Torres");
        response.DocumentoIdentidad.Should().Be("05372334");
        response.Activo.Should().BeTrue();
        repository.Patients.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicatedDocument()
    {
        var repository = new FakePacienteRepository();
        repository.AddExisting(new Paciente("Paciente existente", "05372334"));
        var service = new PacienteServiceImpl(repository, new CreatePacienteValidator());

        var action = () => service.CreateAsync(new CreatePacienteRequest
        {
            Nombre = "Otro paciente",
            DocumentoIdentidad = "05372334"
        });

        await action.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*documento de identidad*");
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidDocument()
    {
        var service = new PacienteServiceImpl(
            new FakePacienteRepository(),
            new CreatePacienteValidator());

        var action = () => service.CreateAsync(new CreatePacienteRequest
        {
            Nombre = "Nelly Torres",
            DocumentoIdentidad = "123"
        });

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowWhenPatientDoesNotExist()
    {
        var service = new PacienteServiceImpl(
            new FakePacienteRepository(),
            new CreatePacienteValidator());

        var action = () => service.GetByIdAsync(99);

        await action.Should().ThrowAsync<NotFoundException>();
    }
}
