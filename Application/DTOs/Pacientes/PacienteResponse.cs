namespace Application.DTOs.Pacientes;

public class PacienteResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string DocumentoIdentidad { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
