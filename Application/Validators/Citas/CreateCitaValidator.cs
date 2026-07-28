using Application.DTOs.Citas;
using FluentValidation;

namespace Application.Validators.Citas;

public class CreateCitaValidator : AbstractValidator<CreateCitaRequest>
{
    public CreateCitaValidator()
    {
        RuleFor(request => request.DoctorId)
            .GreaterThan(0);

        RuleFor(request => request.PacienteId)
            .GreaterThan(0);

        RuleFor(request => request.FechaHoraInicio)
            .NotEmpty();

        RuleFor(request => request.FechaHoraFin)
            .NotEmpty()
            .GreaterThan(request => request.FechaHoraInicio)
            .WithMessage("La fecha y hora final debe ser mayor que la fecha y hora inicial.");
    }
}
