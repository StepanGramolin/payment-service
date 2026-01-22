namespace PaymentService.WebApi.Models;

public sealed class Payment
{
    // ID и одновременно связь с order-service
    public long OrderId { get; set; }

    public decimal Price { get; set; }

    // В таблице/ответе нужен bool
    public bool Status { get; set; }

    public DateTime DateCreate { get; set; } = DateTime.UtcNow;
}