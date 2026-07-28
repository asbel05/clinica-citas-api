using Application.DTOs.Pacientes;
using Domain.Entities;

namespace Application.Mappers;

public static class PacienteMapper
{
    public static Paciente ToEntity(CreatePacienteRequest request)
    {
        return new Paciente(request.Nombre, request.DocumentoIdentidad);
    }

    public static PacienteResponse ToResponse(Paciente paciente)
    {
        return new PacienteResponse
        {
            Id = paciente.Id,
            Nombre = paciente.Nombre,
            DocumentoIdentidad = paciente.DocumentoIdentidad,
            Activo = paciente.Activo
        };
    }
}
