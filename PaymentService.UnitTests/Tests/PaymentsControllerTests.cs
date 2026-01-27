using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.UnitTests.Tests;

[TestFixture]
public class PaymentsControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private PaymentsController _controller;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PaymentsController(_mediatorMock.Object);

        // Мокаем HttpContext для доступа к Request.Headers (для CorrelationId)
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Test]
    public async Task Create_ValidCommand_ShouldReturnOkWithPaymentId()
    {
        // Arrange
        var command = new CreatePaymentCommand(1, 100);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1L);

        // Act
        var result = await _controller.Create(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { paymentId = 1L });
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateStatus_ValidCommand_ShouldReturnNoContent()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePaymentStatusCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);
        _controller.ControllerContext.HttpContext.Request.Headers["X-Correlation-Id"] = "test-corr-id";

        // Act
        var result = await _controller.UpdateStatus(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<UpdatePaymentStatusCommand>(
            c => c.PaymentId == 1 && c.StatusId == 1 && c.CorrelationId == "test-corr-id"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateStatus_NotFoundPayment_ShouldReturnNotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePaymentStatusCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateStatus(999, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdatePaymentStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_ExistingPayment_ShouldReturnOkWithPaymentResponse()
    {
        // Arrange
        var paymentResponse = new GetPaymentResponse(100, true, DateTimeOffset.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPaymentQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(paymentResponse);

        // Act
        var result = await _controller.Get(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        (result as OkObjectResult)!.Value.Should().Be(paymentResponse);
        _mediatorMock.Verify(m => m.Send(It.Is<GetPaymentQuery>(q => q.PaymentId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_NonExistingPayment_ShouldReturnNotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPaymentQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((GetPaymentResponse?)null);

        // Act
        var result = await _controller.Get(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetPaymentQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}