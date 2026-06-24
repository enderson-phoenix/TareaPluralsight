using CalSystem.Application.Orders.Commands.CloseOrder;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Commands;

public class CloseOrderHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly CloseOrderHandler _handler;

    public CloseOrderHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new CloseOrderHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingOrderWithNotes_ReturnsTrueAndPersistsNotes()
    {
        // Arrange
        var order = ServiceOrder.Create("Ana García", "Termómetro PT-100", "Lectura errática");
        var command = new CloseOrderCommand(order.Id, "Sensor calibrado. Lectura corregida a ±0.1°C.");

        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ServiceOrder>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Closed);
        order.Notes.Should().Be("Sensor calibrado. Lectura corregida a ±0.1°C.");
        _repositoryMock.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingOrderWithoutNotes_ReturnsTrueAndClosesWithNullNotes()
    {
        // Arrange
        var order = ServiceOrder.Create("Carlos López", "Balanza XR-200", "No enciende");
        var command = new CloseOrderCommand(order.Id, null);

        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ServiceOrder>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Closed);
        order.Notes.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ReturnsFalse()
    {
        // Arrange
        var command = new CloseOrderCommand(Guid.NewGuid(), "Notas irrelevantes");
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ServiceOrder?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ServiceOrder>()), Times.Never);
    }
}
