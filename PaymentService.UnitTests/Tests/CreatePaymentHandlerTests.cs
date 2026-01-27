using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.Mappers;
using PaymentService.WebApi.UseCases;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.UnitTests.Tests;

[TestFixture]
public class CreatePaymentHandlerTests
{
    private PaymentsDbContext _db;
    private PaymentMapper _mapper;
    private CreatePaymentHandler _handler;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _db = new PaymentsDbContext(options);
        _mapper = new PaymentMapper();
        _handler = new CreatePaymentHandler(_db, _mapper);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Handle_NewPayment_ShouldCreatePaymentAndReturnOrderId()
    {
        var command = new CreatePaymentCommand(OrderId: 10, Price: 250);

        var resultOrderId = await _handler.Handle(command, CancellationToken.None);

        resultOrderId.Should().Be(command.OrderId);
        var payment = await _db.Payments.FindAsync(command.OrderId);
        payment.Should().NotBeNull();
        payment!.Price.Should().Be(command.Price);
        payment.Status.Should().BeFalse(); // По умолчанию при создании статус false
    }

    [Test]
    public async Task Handle_ExistingPayment_ShouldReturnExistingOrderIdWithoutCreatingNew()
    {
        // Arrange
        _db.Payments.Add(new Payment { OrderId = 10, Price = 200, Status = true });
        await _db.SaveChangesAsync();

        var command = new CreatePaymentCommand(OrderId: 10, Price: 250); // Повторная попытка создания

        // Act
        var resultOrderId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultOrderId.Should().Be(command.OrderId);
        // Проверяем, что никаких новых платежей не создалось и старый не изменился
        var paymentCount = await _db.Payments.CountAsync();
        paymentCount.Should().Be(1);
        var existingPayment = await _db.Payments.FindAsync(command.OrderId);
        existingPayment!.Price.Should().Be(200); // Цена не должна измениться
    }
}