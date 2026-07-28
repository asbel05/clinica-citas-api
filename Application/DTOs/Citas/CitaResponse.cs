using Domain.Enums;

namespace Application.DTOs.Citas;

public class CitaResponse
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public int PacienteId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public EstadoCita Estado { get; set; }

    public string? MotivoCancelacion { get; set; }
}
