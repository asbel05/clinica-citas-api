using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Citas;
using Application.DTOs.Doctores;
using Application.DTOs.Pacientes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Integration;

public class ApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        factory.ApplyMigrations();
    }

    [Fact]
    public async Task Swagger_ShouldExposeUiAndOpenApiDocument()
    {
        var uiResponse = await _client.GetAsync("/swagger/index.html");
        var uiContent = await uiResponse.Content.ReadAsStringAsync();

        uiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        uiContent.Should().Contain("Swagger UI");

        var documentResponse = await _client.GetAsync("/swagger/v1/swagger.json");
        var documentContent = await documentResponse.Content.ReadAsStringAsync();

        documentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        documentContent.Should().Contain("/api/citas");
        documentContent.Should().Contain("pacienteDocumentoIdentidad");
    }

    [Fact]
    public async Task Api_ShouldCreateAndListAppointmentWithPublicResponseContract()
    {
        var doctor = await CreateDoctorAsync("API Doctor");
        var patient = await CreatePatientAsync();
        var start = DateTime.UtcNow.AddDays(5);

        var createResponse = await _client.PostAsJsonAsync("/api/citas", new CreateCitaRequest
        {
            DoctorId = doctor.Id,
            PacienteId = patient.Id,
            FechaHoraInicio = start,
            FechaHoraFin = start.AddHours(1)
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var appointment = await createResponse.Content.ReadFromJsonAsync<CitaResponse>();
        appointment.Should().NotBeNull();
        appointment!.DoctorNombre.Should().Be(doctor.Nombre);
        appointment.PacienteNombre.Should().Be(patient.Nombre);
        appointment.PacienteDocumentoIdentidad.Should().Be(patient.DocumentoIdentidad);
        appointment.Estado.Should().Be("Programada");

        appointment.Should().NotBeNull();
        var listResponse = await _client.GetAsync($"/api/citas?doctorId={doctor.Id}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        listResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var appointments = await listResponse.Content.ReadFromJsonAsync<List<CitaResponse>>();
        appointments.Should().ContainSingle(item => item.Id == appointment.Id);
    }

    [Fact]
    public async Task Api_ShouldReturnValidationAndConflictErrors()
    {
        var invalidDoctorResponse = await _client.PostAsJsonAsync("/api/doctores", new CreateDoctorRequest());

        invalidDoctorResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationError = await ReadJsonAsync(invalidDoctorResponse);
        validationError.GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");

        var patientRequest = new CreatePacienteRequest
        {
            Nombre = "Paciente duplicado",
            DocumentoIdentidad = UniqueDocument()
        };

        var firstPatientResponse = await _client.PostAsJsonAsync("/api/pacientes", patientRequest);
        firstPatientResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicatePatientResponse = await _client.PostAsJsonAsync("/api/pacientes", patientRequest);
        duplicatePatientResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var conflictError = await ReadJsonAsync(duplicatePatientResponse);
        conflictError.GetProperty("code").GetString().Should().Be("BUSINESS_RULE_VIOLATION");
    }

    [Fact]
    public async Task Api_ShouldCancelAppointmentAndReturnTextStatus()
    {
        var doctor = await CreateDoctorAsync("API Doctor Cancelacion");
        var patient = await CreatePatientAsync();
        var start = DateTime.UtcNow.AddDays(6);

        var createResponse = await _client.PostAsJsonAsync("/api/citas", new CreateCitaRequest
        {
            DoctorId = doctor.Id,
            PacienteId = patient.Id,
            FechaHoraInicio = start,
            FechaHoraFin = start.AddHours(1)
        });
        var appointment = await createResponse.Content.ReadFromJsonAsync<CitaResponse>();

        var cancelResponse = await _client.PatchAsJsonAsync(
            $"/api/citas/{appointment!.Id}/cancelar",
            new CancelCitaRequest { MotivoCancelacion = "Cambio de horario" });

        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/citas/{appointment.Id}");
        var cancelledAppointment = await getResponse.Content.ReadFromJsonAsync<CitaResponse>();
        cancelledAppointment!.Estado.Should().Be("Cancelada");
        cancelledAppointment.MotivoCancelacion.Should().Be("Cambio de horario");
    }

    [Fact]
    public async Task Api_ShouldExposeAllResourceQueriesAndDoctorDeactivation()
    {
        var doctor = await CreateDoctorAsync("API Doctor Consultas");
        var patient = await CreatePatientAsync();
        var start = DateTime.UtcNow.AddDays(7);

        var createAppointmentResponse = await _client.PostAsJsonAsync("/api/citas", new CreateCitaRequest
        {
            DoctorId = doctor.Id,
            PacienteId = patient.Id,
            FechaHoraInicio = start,
            FechaHoraFin = start.AddHours(1)
        });
        createAppointmentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var doctorsResponse = await _client.GetAsync("/api/doctores");
        doctorsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await doctorsResponse.Content.ReadFromJsonAsync<List<DoctorResponse>>())
            .Should().Contain(item => item.Id == doctor.Id);

        var doctorByIdResponse = await _client.GetAsync($"/api/doctores/{doctor.Id}");
        doctorByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var patientsResponse = await _client.GetAsync("/api/pacientes");
        patientsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await patientsResponse.Content.ReadFromJsonAsync<List<PacienteResponse>>())
            .Should().Contain(item => item.Id == patient.Id);

        var patientByIdResponse = await _client.GetAsync($"/api/pacientes/{patient.Id}");
        patientByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var filteredAppointmentsResponse = await _client.GetAsync(
            $"/api/citas?pacienteId={patient.Id}&estado=Programada");
        filteredAppointmentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await filteredAppointmentsResponse.Content.ReadFromJsonAsync<List<CitaResponse>>())
            .Should().ContainSingle(item => item.PacienteNombre == patient.Nombre);

        var doctorWithoutAppointments = await CreateDoctorAsync("API Doctor Desactivar");
        var deactivateResponse = await _client.PatchAsync(
            $"/api/doctores/{doctorWithoutAppointments.Id}/desactivar",
            content: null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var inactiveDoctorResponse = await _client.GetAsync(
            $"/api/doctores/{doctorWithoutAppointments.Id}");
        var inactiveDoctor = await inactiveDoctorResponse.Content
            .ReadFromJsonAsync<DoctorResponse>();
        inactiveDoctor!.Activo.Should().BeFalse();

        var doctorWithFutureAppointment = await CreateDoctorAsync("API Doctor Conflicto");
        var conflictPatient = await CreatePatientAsync();
        var conflictAppointmentStart = DateTime.UtcNow.AddDays(8);
        var conflictAppointmentResponse = await _client.PostAsJsonAsync("/api/citas", new CreateCitaRequest
        {
            DoctorId = doctorWithFutureAppointment.Id,
            PacienteId = conflictPatient.Id,
            FechaHoraInicio = conflictAppointmentStart,
            FechaHoraFin = conflictAppointmentStart.AddHours(1)
        });
        conflictAppointmentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var conflictDeactivateResponse = await _client.PatchAsync(
            $"/api/doctores/{doctorWithFutureAppointment.Id}/desactivar",
            content: null);
        conflictDeactivateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Api_ShouldReturnNotFoundForUnknownResources()
    {
        var doctorResponse = await _client.GetAsync("/api/doctores/999999");
        var patientResponse = await _client.GetAsync("/api/pacientes/999999");
        var appointmentResponse = await _client.GetAsync("/api/citas/999999");

        doctorResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        patientResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        appointmentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<DoctorResponse> CreateDoctorAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/doctores", new CreateDoctorRequest
        {
            Nombre = $"{name} {Guid.NewGuid():N}",
            Especialidad = "Cardiologia"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<DoctorResponse>())!;
    }

    private async Task<PacienteResponse> CreatePatientAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/pacientes", new CreatePacienteRequest
        {
            Nombre = $"Paciente {Guid.NewGuid():N}",
            DocumentoIdentidad = UniqueDocument()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<PacienteResponse>())!;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }

    private static string UniqueDocument()
    {
        return Random.Shared.Next(10_000_000, 99_999_999).ToString();
    }
}
