using CalSystem.Application.Orders.Commands.CreateOrder;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Commands;

public class CreateOrderHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new CreateOrderHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewOrderId()
    {
        // Arrange
        var command = new CreateOrderCommand(
            CustomerName: "Ana García",
            Equipment: "Termómetro PT-100",
            ProblemDescription: "Lectura errática"
        );

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ServiceOrder>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ServiceOrder>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrderWithPendingStatus()
    {
        // Arrange
        ServiceOrder? capturedOrder = null;
        var command = new CreateOrderCommand("Ana García", "Termómetro PT-100", "Lectura errática");

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ServiceOrder>()))
            .Callback<ServiceOrder>(order => capturedOrder = order)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Status.Should().Be(OrderStatus.Pending);
        capturedOrder.CustomerName.Should().Be("Ana García");
    }
}
