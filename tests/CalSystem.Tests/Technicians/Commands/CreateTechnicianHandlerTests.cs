using CalSystem.Application.Technicians.Commands.CreateTechnician;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Technicians.Commands;

public class CreateTechnicianHandlerTests
{
    private readonly Mock<ITechnicianRepository> _repositoryMock;
    private readonly CreateTechnicianHandler _handler;

    public CreateTechnicianHandlerTests()
    {
        _repositoryMock = new Mock<ITechnicianRepository>();
        _handler = new CreateTechnicianHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewTechnicianId()
    {
        // Arrange
        var command = new CreateTechnicianCommand("Carlos López", "carlos@phoenix.com");
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Technician>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAsyncOnce()
    {
        // Arrange
        var command = new CreateTechnicianCommand("Ana García", "ana@phoenix.com");
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Technician>())).Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Technician>(
            t => t.Name == "Ana García" && t.Email == "ana@phoenix.com"
        )), Times.Once);
    }
}
