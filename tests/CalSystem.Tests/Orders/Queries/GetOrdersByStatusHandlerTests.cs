using CalSystem.Application.Orders.Queries.GetOrdersByStatus;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Queries;

public class GetOrdersByStatusHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly GetOrdersByStatusHandler _handler;

    public GetOrdersByStatusHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new GetOrdersByStatusHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOrdersMappedToDtos()
    {
        // Arrange
        var orders = new List<ServiceOrder>
        {
            ServiceOrder.Create("Cliente A", "Equipo 1", "Problema 1"),
            ServiceOrder.Create("Cliente B", "Equipo 2", "Problema 2")
        };

        _repositoryMock
            .Setup(r => r.GetByStatusAsync(OrderStatus.Pending))
            .ReturnsAsync(orders);

        var query = new GetOrdersByStatusQuery(OrderStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().CustomerName.Should().Be("Cliente A");
        result.All(o => o.Status == "Pending").Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyCollection()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByStatusAsync(OrderStatus.Closed))
            .ReturnsAsync(new List<ServiceOrder>());

        var query = new GetOrdersByStatusQuery(OrderStatus.Closed);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
