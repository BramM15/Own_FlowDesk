using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowDesk.IntegrationTests.Repositories;

public class TicketRepositoryTests : IClassFixture<CustomApiFactory>
{
    private readonly CustomApiFactory _factory;

    public TicketRepositoryTests(CustomApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddAsync_ShouldSaveTicketToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new TicketRepository(dbContext);
        
        // 1. Seed afhankelijkheden (Department & User)
        var department = new Department("IT Support", "Hardware en Software");
        dbContext.Departments.Add(department);
        
        var user = new User("Jan", "Jansen", "jan@support.nl", "hash", UserRole.Support, department.Id);
        dbContext.Users.Add(user);
        
        await dbContext.SaveChangesAsync();

        // 2. Maak de test ticket aan
        var ticket = new Ticket("Netwerk down", "Geen internetverbinding", TicketPriority.Critical, user.Id, department.Id);

        // Act
        await repository.AddAsync(ticket);

        // Assert
        var dbTicket = await dbContext.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(dbTicket);
        Assert.Equal("Netwerk down", dbTicket.Title);
        Assert.Equal(TicketStatus.Open, dbTicket.Status); // Hoort default Open te zijn
        Assert.Equal(user.Id, dbTicket.CreatedByUserId);
    }

    [Fact]
    public async Task GetByAssignedUserAsync_ShouldReturnOnlyAssignedTickets()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new TicketRepository(dbContext);
        
        var department = new Department("Facilitair", "Beheer pand");
        dbContext.Departments.Add(department);
        
        var creator = new User("Piet", "Klaassen", "piet@test.nl", "hash", UserRole.Support, department.Id);
        var assignee = new User("Kees", "Smit", "kees@test.nl", "hash", UserRole.Admin, department.Id);
        dbContext.Users.AddRange(creator, assignee);
        await dbContext.SaveChangesAsync();

        // Ticket 1: Toegewezen aan Kees
        var ticket1 = new Ticket("Lamp stuk", "In de hal", TicketPriority.Low, creator.Id, department.Id);
        ticket1.Update("Lamp stuk", "In de hal", TicketStatus.InProgress, TicketPriority.Low, assignee.Id, department.Id);
        
        // Ticket 2: Niet toegewezen (null)
        var ticket2 = new Ticket("Koffiezetapparaat", "Bonen op", TicketPriority.High, creator.Id, department.Id);
        
        dbContext.Tickets.AddRange(ticket1, ticket2);
        await dbContext.SaveChangesAsync();

        // Act
        var assignedTickets = await repository.GetByAssignedUserAsync(assignee.Id);

        // Assert
        Assert.Single(assignedTickets); // Kees hoort er maar 1 te hebben
        Assert.Equal(ticket1.Id, assignedTickets.First().Id);
    }
}