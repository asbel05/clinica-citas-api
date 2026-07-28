using Application.DTOs.Citas;
using FluentValidation;

namespace Application.Validators.Citas;

public class CancelCitaValidator : AbstractValidator<CancelCitaRequest>
{
    public CancelCitaValidator()
    {
        RuleFor(request => request.MotivoCancelacion)
            .MaximumLength(500)
            .When(request => request.MotivoCancelacion is not null);
    }
}
