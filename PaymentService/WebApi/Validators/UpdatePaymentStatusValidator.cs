using FluentValidation;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.Validators;

public class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithMessage("Payment ID must be a positive number.");

        RuleFor(x => x.StatusId)
            .InclusiveBetween(0, 1)
            .WithMessage("Payment status must be 0 or 1.");

        // Валидация CorrelationId, так как он теперь часть команды
        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("Correlation ID is required.");
    }
}
