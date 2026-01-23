using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.WebApi.Contracts;
using PaymentService.WebApi.Infrastructure;
using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.Mappers;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    private readonly KafkaProducer _producer;
    private readonly PaymentMapper _paymentMapper;

    public PaymentsController(PaymentsDbContext db, KafkaProducer producer, PaymentMapper paymentMapper)
    {
        _db = db;
        _producer = producer;
        _paymentMapper = paymentMapper;
    }

    [HttpPut("updateStatus/{paymentId:long}/{statusId:int}")]
    public async Task<IActionResult> UpdateStatus(long paymentId, int statusId, CancellationToken ct)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(x => x.OrderId == paymentId, ct);
        if (payment is null) return NotFound();

        var newStatus = statusId == 1;

        // если уже был true и снова true Ч событие не шлЄм
        var wasSucceeded = payment.Status;
        payment.Status = newStatus;

        await _db.SaveChangesAsync(ct);

        if (newStatus && !wasSucceeded)
        {
            var correlationId =
                Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
                    ? cid.ToString()
                    : Guid.NewGuid().ToString("N");

            var evt = _paymentMapper.ToPaymentSucceededV1(payment);

            await _producer.ProducePaymentSucceededAsync(evt, correlationId, ct);
        }

        return NoContent();
    }

    // POST api/payments/create
    // body: { orderId: long, price: decimal }
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        // ≈сли платеж уже есть Ч просто вернуть OK
        var existing = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == request.OrderId, ct);
        if (existing is not null)
            return Ok(new { paymentId = existing.OrderId });

        var payment = _paymentMapper.ToPaymentEntity(request);

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        // paymentId == orderId
        return Ok(new { paymentId = payment.OrderId });
    }

    // GET api/payments/get/{paymentId}
    [HttpGet("get/{paymentId:long}")]
    public async Task<IActionResult> Get(long paymentId, CancellationToken ct)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == paymentId, ct);
        if (payment is null) return NotFound();

        return Ok(_paymentMapper.ToGetPaymentResponse(payment));
    }
}

public sealed record CreatePaymentRequest(
    long OrderId, 
    decimal Price);

public sealed record GetPaymentResponse(
    decimal Price,
    bool Status,
    DateTimeOffset DateCreate
);