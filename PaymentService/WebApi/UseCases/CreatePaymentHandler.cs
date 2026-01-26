using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.WebApi.Mappers;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.UseCases;
public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, long>
{
    private readonly PaymentsDbContext _db;
    private readonly PaymentMapper _mapper;

    public CreatePaymentHandler(PaymentsDbContext db, PaymentMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<long> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        var existing = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == request.OrderId, ct);
        if (existing is not null) return existing.OrderId;

        var payment = _mapper.ToPaymentEntity(request);
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        return payment.OrderId;
    }
}
