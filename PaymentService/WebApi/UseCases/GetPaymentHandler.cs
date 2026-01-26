using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.Mappers;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.UseCases;
public class GetPaymentHandler : IRequestHandler<GetPaymentQuery, GetPaymentResponse?>
{
    private readonly PaymentsDbContext _db;
    private readonly PaymentMapper _mapper;

    public GetPaymentHandler(PaymentsDbContext db, PaymentMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<GetPaymentResponse?> Handle(GetPaymentQuery request, CancellationToken ct)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == request.PaymentId, ct);
        if (payment is null) return null;

        return _mapper.ToGetPaymentResponse(payment);
    }
}
