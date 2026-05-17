using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FlowDesk.API.DTOs; // Nodig voor jouw CreateCommentRequest
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowDesk.IntegrationTests.Endpoints;

public class TicketCommentEndpointsTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;
    private readonly CustomApiFactory _factory;

    public TicketCommentEndpointsTests(CustomApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateComment_ReturnsOk_WhenValid()
    {
        // Arrange: Zorg voor een User en Ticket in de database
        Guid ticketId, userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var dept = new Department("HR", "HR");
            db.Departments.Add(dept);
            
            var user = new User("Sam", "A", "sam@hr.com", "hash", UserRole.Support, dept.Id);
            db.Users.Add(user);
            
            var ticket = new Ticket("Ziekteverzuim", "Vraag over systeem", TicketPriority.Low, user.Id, dept.Id);
            db.Tickets.Add(ticket);
            
            await db.SaveChangesAsync();
            
            ticketId = ticket.Id;
            userId = user.Id;
        }

        // Jouw specifieke DTO die alleen Content en UserId bevat
        var request = new CreateCommentRequest("Kan je me hier morgen over bellen?", userId);

        // Act: POST naar de geneste route
        var response = await _client.PostAsJsonAsync($"/tickets/{ticketId}/comments", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); 
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Kan je me hier morgen over bellen?", jsonResponse.GetProperty("content").GetString());
        Assert.Equal(ticketId.ToString(), jsonResponse.GetProperty("ticketId").GetString());
    }

    [Fact]
    public async Task GetByTicketId_ReturnsOk_WithListOfComments()
    {
        // Arrange: Seed Ticket met een comment
        Guid ticketId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dept = new Department("DevOps", "Infra");
            var user = new User("Ops", "User", "ops@dev.com", "hash", UserRole.Support, dept.Id);
            var ticket = new Ticket("Server down", "Geen ping", TicketPriority.Critical, user.Id, dept.Id);
            
            db.Departments.Add(dept);
            db.Users.Add(user);
            db.Tickets.Add(ticket);
            
            var comment = new TicketComment("Onderzoek gestart in de logs.", ticket.Id, user.Id);
            db.TicketComments.Add(comment);
            
            await db.SaveChangesAsync();
            ticketId = ticket.Id;
        }

        // Act: GET request naar de geneste route
        var response = await _client.GetAsync($"/tickets/{ticketId}/comments");

        // Assert
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        // Controleer of we een Array terugkrijgen en of deze minimaal 1 element bevat
        Assert.Equal(JsonValueKind.Array, jsonResponse.ValueKind);
        Assert.True(jsonResponse.GetArrayLength() > 0);
        Assert.Equal("Onderzoek gestart in de logs.", jsonResponse[0].GetProperty("content").GetString());
    }

    [Fact]
    public async Task DeleteComment_ReturnsNoContent_WhenCommentIsDeleted()
    {
        // Arrange: Seed een specifiek comment
        Guid commentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dept = new Department("X", "Y");
            var user = new User("U", "I", "u@i.nl", "h", UserRole.Support, dept.Id);
            var ticket = new Ticket("T", "D", TicketPriority.Low, user.Id, dept.Id);
            var comment = new TicketComment("Typefoutje", ticket.Id, user.Id);
            
            db.Departments.Add(dept);
            db.Users.Add(user);
            db.Tickets.Add(ticket);
            db.TicketComments.Add(comment);
            
            await db.SaveChangesAsync();
            commentId = comment.Id;
        }

        // Act: DELETE naar de specifieke comment endpoint
        var response = await _client.DeleteAsync($"/tickets/comments/{commentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 
    }
}