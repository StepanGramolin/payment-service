using FluentValidation;
using PaymentService.WebApi.Controllers;


namespace PaymentService.WebApi.Validators;

public class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithMessage("ID платежа должен быть положительным числом");

        RuleFor(x => x.StatusId)
            .InclusiveBetween(0, 2)
            .WithMessage("Статус платежа должен быть в диапазоне от 0 до 2");
    }
}
