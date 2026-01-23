namespace PaymentService.DataAccess.Postgres.Models;

public sealed class Payment
{
    // ID и одновременно связь с order-service
    public long OrderId { get; set; }

    public decimal Price { get; set; }

    public bool Status { get; set; }

    public DateTimeOffset DateCreate { get; set; } = DateTime.UtcNow;
}