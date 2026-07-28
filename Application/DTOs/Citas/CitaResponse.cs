namespace Application.DTOs.Citas;

public class CitaResponse
{
    public int Id { get; set; }

    public string DoctorNombre { get; set; } = string.Empty;

    public string DoctorEspecialidad { get; set; } = string.Empty;

    public string PacienteNombre { get; set; } = string.Empty;

    public string PacienteDocumentoIdentidad { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string? MotivoCancelacion { get; set; }
}
