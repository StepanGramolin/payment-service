using FluentValidation;
using PaymentService.WebApi.UseCases;

namespace PaymentService.WebApi.Validators;

public class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithMessage("Payment ID must be a positive number.");

        RuleFor(x => x.StatusId)
            .InclusiveBetween(0, 2)
            .WithMessage("Payment status must be between 0 and 2.");

        // Валидация CorrelationId, так как он теперь часть команды
        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("Correlation ID is required.");
    }
}
