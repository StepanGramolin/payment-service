using MediatR;
using Microsoft.EntityFrameworkCore;

using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.WebApi.UseCases.Commands;
using PaymentService.WebApi.Infrastructure;
using PaymentService.WebApi.Mappers;

namespace PaymentService.WebApi.UseCases;

public class UpdatePaymentStatusHandler : IRequestHandler<UpdatePaymentStatusCommand, bool>
{
    private readonly PaymentsDbContext _db;
    private readonly IKafkaProducer _producer;
    private readonly PaymentMapper _mapper;

    public UpdatePaymentStatusHandler(PaymentsDbContext db, IKafkaProducer producer, PaymentMapper mapper)
    {
        _db = db;
        _producer = producer;
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdatePaymentStatusCommand request, CancellationToken ct)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(x => x.OrderId == request.PaymentId, ct);
        if (payment is null) return false;

        var newStatus = request.StatusId == 1;
        var wasSucceeded = payment.Status;

        payment.Status = newStatus;
        await _db.SaveChangesAsync(ct);

        // Логика отправки события
        if (newStatus && !wasSucceeded)
        {
            var evt = _mapper.ToPaymentSucceededV1(payment);
            await _producer.ProducePaymentSucceededAsync(evt, request.CorrelationId, ct);
        }

        return true;
    }
}
