using FluentValidation;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        // Валидация OrderId
        RuleFor(request => request.OrderId)
            .NotEmpty().WithMessage("Order ID is required.")
            .GreaterThan(0).WithMessage("Order ID must be a positive number.");

        // Валидация Price
        RuleFor(request => request.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
