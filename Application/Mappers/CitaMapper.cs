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

    public static CitaResponse ToResponse(Cita cita)
    {
        return new CitaResponse
        {
            Id = cita.Id,
            DoctorId = cita.DoctorId,
            PacienteId = cita.PacienteId,
            FechaHoraInicio = cita.FechaHoraInicio,
            FechaHoraFin = cita.FechaHoraFin,
            Estado = cita.Estado,
            MotivoCancelacion = cita.MotivoCancelacion
        };
    }
}
