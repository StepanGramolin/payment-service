using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using FluentAssertions;

using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.DataAccess.Postgres.Models;
using PaymentService.WebApi.Mappers;
using PaymentService.WebApi.UseCases;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.UnitTests.Tests;

[TestFixture]
public class GetPaymentHandlerTests
{
    private PaymentsDbContext _db;
    private PaymentMapper _mapper;
    private GetPaymentHandler _handler;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _db = new PaymentsDbContext(options);
        _mapper = new PaymentMapper();
        _handler = new GetPaymentHandler(_db, _mapper);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public async Task Handle_ExistingPaymentId_ShouldReturnPaymentResponse()
    {
        var payment = new Payment { OrderId = 1, Price = 100, Status = true, DateCreate = DateTimeOffset.UtcNow };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        var query = new GetPaymentQuery(1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Price.Should().Be(payment.Price);
        result.Status.Should().Be(payment.Status);
    }

    [Test]
    public async Task Handle_NonExistingPaymentId_ShouldReturnNull()
    {
        var query = new GetPaymentQuery(999);
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().BeNull();
    }
}