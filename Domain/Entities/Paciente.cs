namespace Domain.Entities;

public class Paciente
{
    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string DocumentoIdentidad { get; private set; } = string.Empty;

    public bool Activo { get; private set; } = true;

    private Paciente()
    {
    }

    public Paciente(string nombre, string documentoIdentidad)
    {
        Nombre = nombre;
        DocumentoIdentidad = documentoIdentidad;
    }
}
