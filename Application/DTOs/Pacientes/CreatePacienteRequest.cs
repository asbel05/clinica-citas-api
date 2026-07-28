namespace Application.DTOs.Pacientes;

public class CreatePacienteRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string DocumentoIdentidad { get; set; } = string.Empty;
}
