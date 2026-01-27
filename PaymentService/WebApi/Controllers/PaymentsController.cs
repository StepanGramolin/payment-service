using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPut("updateStatus/{paymentId:long}/{statusId:int}")]
    public async Task<IActionResult> UpdateStatus(long paymentId, int statusId, CancellationToken ct)
    {
        // »звлекаем коррел€цию здесь
        var correlationId = Request.Headers.TryGetValue("X-Correlation-Id", out var cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString("N");

        var command = new UpdatePaymentStatusCommand(paymentId, statusId, correlationId);
        var result = await _mediator.Send(command, ct);

        return result ? NoContent() : NotFound();
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command, CancellationToken ct)
    {
        var paymentId = await _mediator.Send(command, ct);
        return Ok(new { paymentId });
    }

    [HttpGet("get/{paymentId:long}")]
    public async Task<IActionResult> Get(long paymentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentQuery(paymentId), ct);
        return result is null ? NotFound() : Ok(result);
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