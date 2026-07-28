using Application.DTOs.Citas;
using Domain.Entities;

namespace Application.Mappers;

public static class CitaMapper
{
    public static Cita ToEntity(CreateCitaRequest request)
    {
        return new Cita(
            request.DoctorId,
            request.PacienteId,
            request.FechaHoraInicio,
            request.FechaHoraFin);
    }

    public static CitaResponse ToResponse(Cita cita, Doctor? doctor = null, Paciente? paciente = null)
    {
        doctor ??= cita.Doctor;
        paciente ??= cita.Paciente;

        return new CitaResponse
        {
            Id = cita.Id,
            DoctorNombre = doctor?.Nombre ?? string.Empty,
            DoctorEspecialidad = doctor?.Especialidad ?? string.Empty,
            PacienteNombre = paciente?.Nombre ?? string.Empty,
            PacienteDocumentoIdentidad = paciente?.DocumentoIdentidad ?? string.Empty,
            FechaHoraInicio = cita.FechaHoraInicio,
            FechaHoraFin = cita.FechaHoraFin,
            Estado = cita.Estado.ToString(),
            MotivoCancelacion = cita.MotivoCancelacion
        };
    }
}
