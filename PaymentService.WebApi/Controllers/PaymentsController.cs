using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.Contracts;
using PaymentService.WebApi.Infrastructure;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly KafkaProducer _producer;

    public PaymentsController(KafkaProducer producer)
    {
        _producer = producer;
    }

    // POST /api/payments/succeed
    [HttpPost("succeed")]
    public async Task<IActionResult> Succeed([FromBody] SucceedPaymentRequest request, CancellationToken ct)
    {
        var correlationId =
            Request.Headers.TryGetValue("X-Correlation-Id", out var cid) && !string.IsNullOrWhiteSpace(cid)
                ? cid.ToString()
                : Guid.NewGuid().ToString("N");

        var evt = new PaymentSucceededV1(
            PaymentId: Guid.NewGuid(),
            OrderId: request.OrderId,
            Amount: request.Amount,
            Currency: request.Currency,
            OccurredAtUtc: DateTimeOffset.UtcNow
        );

        await _producer.ProducePaymentSucceededAsync(evt, correlationId, ct);

        return Accepted(new { correlationId, evt });
    }
}

public sealed record SucceedPaymentRequest(Guid OrderId, decimal Amount, string Currency);