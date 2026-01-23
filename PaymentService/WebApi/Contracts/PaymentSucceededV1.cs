namespace PaymentService.WebApi.Contracts;

public sealed record PaymentSucceededV1(
    long OrderId,
    decimal Price,
    bool Status,
    DateTimeOffset DateCreate
);