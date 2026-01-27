using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using FluentAssertions;

using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.UseCases;
using PaymentService.WebApi.UseCases.Commands;
using PaymentService.WebApi.Infrastructure;
using PaymentService.WebApi.Mappers;
using PaymentService.WebApi.Contracts;

namespace PaymentService.UnitTests.Tests;

[TestFixture]
public class UpdatePaymentStatusHandlerTests
{
    private PaymentsDbContext _db;
    private Mock<IKafkaProducer> _kafkaMock;
    private PaymentMapper _mapper;
    private UpdatePaymentStatusHandler _handler;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _db = new PaymentsDbContext(options);

        _kafkaMock = new Mock<IKafkaProducer>();
        _mapper = new PaymentMapper();
        _handler = new UpdatePaymentStatusHandler(_db, _kafkaMock.Object, _mapper);
    }

    [Test]
    public async Task Handle_WhenStatusBecomesSuccess_ShouldProduceKafkaMessage()
    {
        // Arrange: создаем платеж со статусом false
        var payment = new Payment { OrderId = 1, Price = 100, Status = false };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        var command = new UpdatePaymentStatusCommand(1, 1, "test-corr-id"); // 1 = Success

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        // Проверяем, что KafkaProducer был вызван
        _kafkaMock.Verify(x => x.ProducePaymentSucceededAsync(
            It.IsAny<PaymentSucceededV1>(),
            "test-corr-id",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

