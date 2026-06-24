using CalSystem.Application.Orders.Commands.AssignTechnician;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Commands;

public class AssignTechnicianHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly AssignTechnicianHandler _handler;

    public AssignTechnicianHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new AssignTechnicianHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingOrder_ReturnsTrueAndUpdatesStatus()
    {
        // Arrange
        var order = ServiceOrder.Create("Cliente", "Equipo", "Problema");
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(order.Id, technicianId);

        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ServiceOrder>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.TechnicianId.Should().Be(technicianId);
        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ReturnsFalse()
    {
        // Arrange
        var command = new AssignTechnicianCommand(Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ServiceOrder?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
