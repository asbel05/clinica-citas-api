namespace Domain.Entities;

public class Doctor
{
    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Especialidad { get; private set; } = string.Empty;

    public bool Activo { get; private set; } = true;

    private Doctor()
    {
    }

    public Doctor(string nombre, string especialidad)
    {
        Nombre = nombre;
        Especialidad = especialidad;
    }
}
