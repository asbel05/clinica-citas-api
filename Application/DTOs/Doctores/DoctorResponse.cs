namespace Application.DTOs.Doctores;

public class DoctorResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Especialidad { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
