using MediatR;
using PaymentService.WebApi.Controllers;

namespace PaymentService.WebApi.UseCases.Commands;
public record GetPaymentQuery(long PaymentId) : IRequest<GetPaymentResponse?>;