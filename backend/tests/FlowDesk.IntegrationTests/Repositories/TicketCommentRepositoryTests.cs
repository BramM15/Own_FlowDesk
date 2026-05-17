using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowDesk.IntegrationTests.Repositories;

public class TicketCommentRepositoryTests : IClassFixture<CustomApiFactory>
{
    private readonly CustomApiFactory _factory;

    public TicketCommentRepositoryTests(CustomApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddAsync_ShouldSaveCommentToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new TicketCommentRepository(dbContext);
        
        // 1. Seed de complete afhankelijkheidsketen
        var department = new Department("Support", "Klantenservice");
        dbContext.Departments.Add(department);
        
        var user = new User("Lisa", "Smit", "lisa@support.nl", "hash", UserRole.Support, department.Id);
        dbContext.Users.Add(user);
        
        var ticket = new Ticket("Wachtwoord reset", "Klant is wachtwoord vergeten", TicketPriority.Medium, user.Id, department.Id);
        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        // 2. Maak het comment aan
        var comment = new TicketComment("Ik heb een reset-link gestuurd.", ticket.Id, user.Id);

        // Act
        await repository.AddAsync(comment);

        // Assert
        var dbComment = await dbContext.TicketComments.FindAsync(comment.Id);
        Assert.NotNull(dbComment);
        Assert.Equal("Ik heb een reset-link gestuurd.", dbComment.Content);
        Assert.Equal(ticket.Id, dbComment.TicketId);
        Assert.Equal(user.Id, dbComment.UserId);
    }

    [Fact]
    public async Task GetByTicketIdAsync_ShouldReturnOnlyCommentsForSpecificTicket()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new TicketCommentRepository(dbContext);
        
        var department = new Department("IT", "IT Services");
        dbContext.Departments.Add(department);
        
        var user = new User("Tom", "Tech", "tom@it.nl", "hash", UserRole.Admin, department.Id);
        dbContext.Users.Add(user);
        
        var ticket1 = new Ticket("Ticket 1", "Desc 1", TicketPriority.Low, user.Id, department.Id);
        var ticket2 = new Ticket("Ticket 2", "Desc 2", TicketPriority.Low, user.Id, department.Id);
        dbContext.Tickets.AddRange(ticket1, ticket2);
        await dbContext.SaveChangesAsync();

        // Voeg 2 comments toe aan ticket 1, en 1 comment aan ticket 2
        var comment1 = new TicketComment("Comment A op Ticket 1", ticket1.Id, user.Id);
        var comment2 = new TicketComment("Comment B op Ticket 1", ticket1.Id, user.Id);
        var comment3 = new TicketComment("Comment op Ticket 2", ticket2.Id, user.Id);
        
        dbContext.TicketComments.AddRange(comment1, comment2, comment3);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByTicketIdAsync(ticket1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(ticket1.Id, c.TicketId));
    }
}