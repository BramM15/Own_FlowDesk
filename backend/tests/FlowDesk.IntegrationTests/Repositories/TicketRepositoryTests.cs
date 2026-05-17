using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.Extensions.DependencyInjection;

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
        var ticket = new Ticket("Netwerk down", "Geen internetverbinding", TicketPriority.Critical, user.Id,
            department.Id);

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
        ticket1.Update("Lamp stuk", "In de hal", TicketStatus.InProgress, TicketPriority.Low, assignee.Id,
            department.Id);

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

    [Fact]
    public async Task GetByDepartmentAsync_ShouldReturnOnlyTicketsForGivenDepartment()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new TicketRepository(dbContext);

        // Seed afhankelijkheden (2 verschillende departementen en 1 gebruiker)
        var targetDepartment = new Department("IT Helpdesk", "Eerste lijns support");
        var otherDepartment = new Department("HR", "Personeelszaken");
        dbContext.Departments.AddRange(targetDepartment, otherDepartment);

        var user = new User("Anna", "De Vries", "anna@test.nl", "hash", UserRole.Support, targetDepartment.Id);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Ticket 1 & 2: Horen bij het doel-departement (targetDepartment)
        var ticket1 = new Ticket("Muis kapot", "Linkerknop werkt niet", TicketPriority.Low, user.Id,
            targetDepartment.Id);
        var ticket2 = new Ticket("Monitor flikkert", "Sinds de update gisteren", TicketPriority.Medium, user.Id,
            targetDepartment.Id);

        // Ticket 3: Hoort bij een ander departement (otherDepartment)
        var ticket3 = new Ticket("Urenregistratie", "Vraag over verlof", TicketPriority.Low, user.Id,
            otherDepartment.Id);

        dbContext.Tickets.AddRange(ticket1, ticket2, ticket3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByDepartmentAsync(targetDepartment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(targetDepartment.Id, t.DepartmentId));
        Assert.Contains(result, t => t.Id == ticket1.Id);
        Assert.Contains(result, t => t.Id == ticket2.Id);
    }

    [Fact]
    public async Task GetByCreatedUserAsync_ShouldReturnOnlyTicketsCreatedByGivenUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new TicketRepository(dbContext);

        // Seed afhankelijkheden (1 departement en 2 verschillende gebruikers)
        var department = new Department("Finance", "Financiële administratie");
        dbContext.Departments.Add(department);

        var targetUser = new User("Tom", "Bakker", "tom@test.nl", "hash", UserRole.Support, department.Id);
        var otherUser = new User("Lisa", "Groen", "lisa@test.nl", "hash", UserRole.Support, department.Id);
        dbContext.Users.AddRange(targetUser, otherUser);
        await dbContext.SaveChangesAsync();

        // Ticket 1: Aangemaakt door targetUser
        var ticket1 = new Ticket("Factuur onjuist", "Bedrag klopt niet met offerte", TicketPriority.High, targetUser.Id,
            department.Id);

        // Ticket 2: Aangemaakt door otherUser
        var ticket2 = new Ticket("Declaratie software", "Adobe licentie indienen", TicketPriority.Low, otherUser.Id,
            department.Id);

        dbContext.Tickets.AddRange(ticket1, ticket2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByCreatedUserAsync(targetUser.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(ticket1.Id, result.First().Id);
        Assert.Equal(targetUser.Id, result.First().CreatedByUserId);
    }
}