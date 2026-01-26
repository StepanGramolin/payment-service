using FluentValidation;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.Validators;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.")
            .GreaterThan(0).WithMessage("Order ID must be a positive number.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
