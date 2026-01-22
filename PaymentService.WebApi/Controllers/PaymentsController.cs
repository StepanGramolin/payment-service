using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.WebApi.Contracts;
using PaymentService.WebApi.Infrastructure;
using PaymentService.WebApi.Models;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    private readonly KafkaProducer _producer;

    public PaymentsController(PaymentsDbContext db, KafkaProducer producer)
    {
        _db = db;
        _producer = producer;
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

            var evt = new PaymentSucceededV1(
                PaymentId: payment.OrderId,             // paymentId == orderId
                OrderId: payment.OrderId,
                Price: payment.Price,
                OccurredAtUtc: DateTimeOffset.UtcNow
            );

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

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Price = request.Price,
            Status = false,
            DateCreate = DateTime.UtcNow
        };

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

        return Ok(new
        {
            price = payment.Price,
            status = payment.Status,
            dateCreate = payment.DateCreate
        });
    }
}

public sealed record CreatePaymentRequest(long OrderId, decimal Price);