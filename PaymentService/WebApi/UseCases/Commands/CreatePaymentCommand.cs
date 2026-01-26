using MediatR;


namespace PaymentService.WebApi.UseCases.Commands;
public record CreatePaymentCommand(long OrderId, decimal Price) : IRequest<long>;
