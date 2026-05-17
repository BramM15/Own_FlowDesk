using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Xunit;

namespace FlowDesk.UnitTests.Domain;

public class TicketTests
{
    [Fact]
    public void Constructor_ShouldSetProperties_AndSetDefaults()
    {
        // Arrange
        var title = "Laptop start niet op";
        var description = "Zwart scherm bij het aanzetten.";
        var priority = TicketPriority.High;
        var createdByUserId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        // Act
        var ticket = new Ticket(title, description, priority, createdByUserId, departmentId);

        // Assert
        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(title, ticket.Title);
        Assert.Equal(description, ticket.Description);
        Assert.Equal(TicketStatus.Open, ticket.Status); // Status moet standaard Open zijn
        Assert.Equal(priority, ticket.Priority);
        Assert.Equal(createdByUserId, ticket.CreatedByUserId);
        Assert.Equal(departmentId, ticket.DepartmentId);
        Assert.Null(ticket.AssignedToUserId);
        Assert.Null(ticket.ClosedAt);
        
        // Controleer of de tijden ongeveer 'nu' zijn ingesteld
        Assert.True((DateTime.UtcNow - ticket.CreatedAt).TotalSeconds < 1);
        Assert.True((DateTime.UtcNow - ticket.UpdatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Update_ShouldChangeProperties_AndRefreshUpdatedAt()
    {
        // Arrange
        var ticket = new Ticket("Oud", "Oud", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());
        var oldUpdatedAt = ticket.UpdatedAt;
        
        // Wacht heel even zodat we zeker weten dat UpdatedAt verandert
        Thread.Sleep(10); 

        var assignedUserId = Guid.NewGuid();
        var newDepartmentId = Guid.NewGuid();

        // Act
        ticket.Update("Nieuw", "Nieuw", TicketStatus.InProgress, TicketPriority.Medium, assignedUserId, newDepartmentId);

        // Assert
        Assert.Equal("Nieuw", ticket.Title);
        Assert.Equal("Nieuw", ticket.Description);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(TicketPriority.Medium, ticket.Priority);
        Assert.Equal(assignedUserId, ticket.AssignedToUserId);
        Assert.Equal(newDepartmentId, ticket.DepartmentId);
        Assert.True(ticket.UpdatedAt > oldUpdatedAt); // UpdatedAt moet vernieuwd zijn
    }

    [Fact]
    public void Update_ShouldSetClosedAt_WhenStatusIsChangedToClosed()
    {
        // Arrange
        var ticket = new Ticket("T", "D", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());

        // Act
        ticket.Update("T", "D", TicketStatus.Closed, TicketPriority.Low, null, ticket.DepartmentId);

        // Assert
        Assert.NotNull(ticket.ClosedAt);
    }

    [Fact]
    public void Update_ShouldClearClosedAt_WhenStatusIsChangedFromClosedToOpen()
    {
        // Arrange
        var ticket = new Ticket("T", "D", TicketPriority.Low, Guid.NewGuid(), Guid.NewGuid());
        // Sluit eerst het ticket
        ticket.Update("T", "D", TicketStatus.Closed, TicketPriority.Low, null, ticket.DepartmentId);
        Assert.NotNull(ticket.ClosedAt);

        // Act: Heropen het ticket
        ticket.Update("T", "D", TicketStatus.Open, TicketPriority.Low, null, ticket.DepartmentId);

        // Assert
        Assert.Null(ticket.ClosedAt);
    }
}