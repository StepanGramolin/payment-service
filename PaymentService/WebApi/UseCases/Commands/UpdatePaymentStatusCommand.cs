using MediatR;

namespace PaymentService.WebApi.UseCases.Commands;

public record UpdatePaymentStatusCommand(
    long PaymentId,
    int StatusId,
    string CorrelationId // Передаем из контроллера
) : IRequest<bool>; // Возвращаем bool (найден/не найден)