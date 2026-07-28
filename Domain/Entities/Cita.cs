using Domain.Enums;

namespace Domain.Entities;

public class Cita
{
    public int Id { get; private set; }

    public int DoctorId { get; private set; }

    public int PacienteId { get; private set; }

    public DateTime FechaHoraInicio { get; private set; }

    public DateTime FechaHoraFin { get; private set; }

    public EstadoCita Estado { get; private set; } = EstadoCita.Programada;

    public string? MotivoCancelacion { get; private set; }

    private Cita()
    {
    }

    public Cita(int doctorId, int pacienteId, DateTime fechaHoraInicio, DateTime fechaHoraFin)
    {
        DoctorId = doctorId;
        PacienteId = pacienteId;
        FechaHoraInicio = fechaHoraInicio;
        FechaHoraFin = fechaHoraFin;
    }
}
