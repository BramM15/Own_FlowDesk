using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Moq;
using Xunit;

namespace FlowDesk.UnitTests;

public class TicketHandlerTests
{
    private readonly Mock<ITicketRepository> _repositoryMock;
    private readonly TicketHandler _handler;

    public TicketHandlerTests()
    {
        _repositoryMock = new Mock<ITicketRepository>();
        _handler = new TicketHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedTicketWithCorrectValues()
    {
        // Arrange
        var title = "Test Title";
        var description = "Test Description";
        var priority = TicketPriority.High;
        var userId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Ticket>()))
            .ReturnsAsync((Ticket t) => t);

        // Act
        var result = await _handler.CreateAsync(title, description, priority, userId, departmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(description, result.Description);
        Assert.Equal(TicketStatus.Open, result.Status);
        Assert.Equal(priority, result.Priority);
        Assert.Equal(userId, result.CreatedByUserId);
        Assert.Equal(departmentId, result.DepartmentId);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketExists_ShouldModifyPropertiesAndCallUpdate()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var existingTicket = new Ticket("Old Title", "Old Desc", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());
        
        _repositoryMock.Setup(x => x.GetAsync(ticketId)).ReturnsAsync(existingTicket);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Ticket>())).ReturnsAsync((Ticket t) => t);

        var newTitle = "New Title";
        var newDesc = "New Desc";
        var newStatus = TicketStatus.InProgress;
        var newPriority = TicketPriority.Critical;
        var assignedUserId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        // Act
        var result = await _handler.UpdateAsync(ticketId, newTitle, newDesc, newStatus, newPriority, assignedUserId, departmentId);

        // Assert
        Assert.Equal(newTitle, result.Title);
        Assert.Equal(newDesc, result.Description);
        Assert.Equal(newStatus, result.Status);
        Assert.Equal(newPriority, result.Priority);
        Assert.Equal(assignedUserId, result.AssignedToUserId);
        _repositoryMock.Verify(x => x.UpdateAsync(existingTicket), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketDoesNotExist_ShouldThrowException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Ticket?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _handler.UpdateAsync(Guid.NewGuid(), "T", "D", TicketStatus.Closed, TicketPriority.Medium, null, Guid.NewGuid()));
    }
}