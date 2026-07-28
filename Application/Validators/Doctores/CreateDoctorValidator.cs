using Application.DTOs.Doctores;
using FluentValidation;

namespace Application.Validators.Doctores;

public class CreateDoctorValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Especialidad)
            .NotEmpty()
            .MaximumLength(100);
    }
}
