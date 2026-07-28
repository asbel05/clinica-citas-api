using Application.DTOs.Pacientes;
using FluentValidation;

namespace Application.Validators.Pacientes;

public class CreatePacienteValidator : AbstractValidator<CreatePacienteRequest>
{
    public CreatePacienteValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.DocumentoIdentidad)
            .NotEmpty()
            .Matches("^[0-9]{8}$")
            .WithMessage("El documento de identidad debe tener exactamente 8 dígitos.");
    }
}
