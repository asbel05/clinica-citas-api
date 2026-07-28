namespace Application.DTOs.Citas;

public class CreateCitaRequest
{
    public int DoctorId { get; set; }

    public int PacienteId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }
}
