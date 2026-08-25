using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FlowDesk.UnitTests.Application;

public class TicketHandlerTests
{
    private readonly TicketHandler _handler;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ITicketRepository> _mockRepo;

    public TicketHandlerTests()
    {
        _mockRepo = new Mock<ITicketRepository>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        
        _handler = new TicketHandler(_mockRepo.Object, _mockHttpContextAccessor.Object);
    }

    // --- GET METHODS ---

    [Fact]
    public async Task GetAsync_ShouldReturnTicket_WhenExists()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var expectedTicket = new Ticket("Test", "Desc", TicketPriority.High, Guid.NewGuid(), Guid.NewGuid());
        _mockRepo.Setup(r => r.GetAsync(ticketId)).ReturnsAsync(expectedTicket);

        // Act
        var result = await _handler.GetAsync(ticketId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnTickets()
    {
        // Arrange
        var tickets = new List<Ticket>
        {
            new("T1", "D1", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid()),
            new("T2", "D2", TicketPriority.High, Guid.NewGuid(), Guid.NewGuid())
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(tickets);

        // Act
        var result = await _handler.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByDepartmentAsync_ShouldReturnTicketsForDepartment()
    {
        // Arrange
        var deptId = Guid.NewGuid();
        var tickets = new List<Ticket> { new("T", "D", TicketPriority.Low, Guid.NewGuid(), deptId) };
        _mockRepo.Setup(r => r.GetByDepartmentAsync(deptId)).ReturnsAsync(tickets);

        // Act
        var result = await _handler.GetByDepartmentAsync(deptId);

        // Assert
        Assert.Single(result);
        Assert.Equal(deptId, result.First().DepartmentId);
    }

    [Fact]
    public async Task GetByCreatedUserAsync_ShouldReturnTickets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tickets = new List<Ticket> { new("T", "D", TicketPriority.Low, userId, Guid.NewGuid()) };
        _mockRepo.Setup(r => r.GetByCreatedUserAsync(userId)).ReturnsAsync(tickets);

        // Act
        var result = await _handler.GetByCreatedUserAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal(userId, result.First().CreatedByUserId);
    }

    [Fact]
    public async Task GetByAssignedUserAsync_ShouldReturnTickets()
    {
        // Arrange
        var assignedUserId = Guid.NewGuid();
        var ticket = new Ticket("T", "D", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());
        ticket.Update("T", "D", TicketStatus.InProgress, TicketPriority.Low, assignedUserId,
            ticket.DepartmentId); // Assign user

        var tickets = new List<Ticket> { ticket };
        _mockRepo.Setup(r => r.GetByAssignedUserAsync(assignedUserId)).ReturnsAsync(tickets);

        // Act
        var result = await _handler.GetByAssignedUserAsync(assignedUserId);

        // Assert
        Assert.Single(result);
        Assert.Equal(assignedUserId, result.First().AssignedToUserId);
    }

    // --- CREATE METHODS ---

    [Fact]
    public async Task CreateAsync_ShouldCreateAndSaveTicket()
    {
        // Arrange
        var title = "Muis is stuk";
        var description = "De linkermuisknop doet het niet";
        var priority = TicketPriority.Medium;
        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Ticket>()))
            .ReturnsAsync((Ticket t) => t);

        // Act
        var result = await _handler.CreateAsync(title, description, priority, userId, deptId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(TicketStatus.Open, result.Status);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
    }

    // --- UPDATE METHODS ---

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetAsync(ticketId)).ReturnsAsync((Ticket?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.UpdateAsync(ticketId, "T", "D", TicketStatus.Open, TicketPriority.Low, null, Guid.NewGuid()));

        Assert.Equal("Ticket not found", exception.Message);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Ticket>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndSaveTicket_WhenTicketExists()
    {
        // Arrange
        var ticket = new Ticket("Oud", "Oud", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());
        _mockRepo.Setup(r => r.GetAsync(ticket.Id)).ReturnsAsync(ticket);

        // Act
        var result = await _handler.UpdateAsync(ticket.Id, "Nieuw", "Nieuw", TicketStatus.InProgress,
            TicketPriority.High, null, ticket.DepartmentId);

        // Assert
        Assert.Equal("Nieuw", result.Title);
        Assert.Equal(TicketStatus.InProgress, result.Status);
        Assert.Equal(TicketPriority.High, result.Priority);

        _mockRepo.Verify(r => r.UpdateAsync(ticket), Times.Once);
    }

    // --- DELETE METHODS ---

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenTicketDoesNotExist()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetAsync(ticketId)).ReturnsAsync((Ticket?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.DeleteAsync(ticketId));

        Assert.Equal("Ticket not found", exception.Message);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTicket_WhenTicketExists()
    {
        // Arrange
        var ticket = new Ticket("T", "D", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());
        _mockRepo.Setup(r => r.GetAsync(ticket.Id)).ReturnsAsync(ticket);

        // Act
        await _handler.DeleteAsync(ticket.Id);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(ticket.Id), Times.Once);
    }
}