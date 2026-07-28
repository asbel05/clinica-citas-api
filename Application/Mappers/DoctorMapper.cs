using Application.DTOs.Doctores;
using Domain.Entities;

namespace Application.Mappers;

public static class DoctorMapper
{
    public static Doctor ToEntity(CreateDoctorRequest request)
    {
        return new Doctor(request.Nombre, request.Especialidad);
    }

    public static DoctorResponse ToResponse(Doctor doctor)
    {
        return new DoctorResponse
        {
            Id = doctor.Id,
            Nombre = doctor.Nombre,
            Especialidad = doctor.Especialidad,
            Activo = doctor.Activo
        };
    }
}
