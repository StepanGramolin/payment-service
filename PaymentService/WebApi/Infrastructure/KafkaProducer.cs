using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using PaymentService.WebApi.Contracts;

namespace PaymentService.WebApi.Infrastructure;

public sealed class KafkaProducer : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaProducer(IConfiguration configuration)
    {
        var bootstrap = configuration["Kafka:BootstrapServers"]
                        ?? throw new InvalidOperationException("Kafka:BootstrapServers is required");

        _topic = configuration["Kafka:NotificationsTopic"] ?? "notifications";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrap,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageTimeoutMs = 5000,
            ClientId = configuration["Kafka:ClientId"] ?? "payment-service"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProducePaymentSucceededAsync(PaymentSucceededV1 evt, string correlationId, CancellationToken ct)
    {
        var key = evt.OrderId.ToString();
        var value = JsonSerializer.Serialize(evt);

        var msg = new Message<string, string>
        {
            Key = key,
            Value = value,
            Headers = new Headers
            {
                { "correlationId", Encoding.UTF8.GetBytes(correlationId) },
                { "eventType", Encoding.UTF8.GetBytes("PaymentSucceededV1") },
                { "schemaVersion", Encoding.UTF8.GetBytes("1") },
            }
        };

        // ProduceAsync doesn't accept CancellationToken; we respect ct by checking it before/after.
        ct.ThrowIfCancellationRequested();
        await _producer.ProduceAsync(_topic, msg);
        ct.ThrowIfCancellationRequested();

        _producer.Flush(TimeSpan.FromSeconds(2));
    }

    public void Dispose() => _producer.Dispose();
}