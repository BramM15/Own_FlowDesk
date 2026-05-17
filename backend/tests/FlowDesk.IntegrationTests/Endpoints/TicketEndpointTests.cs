using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FlowDesk.API.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowDesk.IntegrationTests.Endpoints;

public class TicketEndpointsTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;
    private readonly CustomApiFactory _factory;

    public TicketEndpointsTests(CustomApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTicket_ReturnsOk_WhenValid()
    {
        // Arrange: Zorg voor een User en Department in de DB
        Guid departmentId, userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var department = new Department("HR", "Human Resources");
            db.Departments.Add(department);
            
            var user = new User("Anna", "A", "anna@hr.com", "hash", UserRole.Support, department.Id);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            
            departmentId = department.Id;
            userId = user.Id;
        }

        var request = new CreateTicketRequest(
            "Vakantiedagen", 
            "Saldo klopt niet", 
            TicketPriority.Medium, 
            userId, 
            departmentId);

        // Act
        var response = await _client.PostAsJsonAsync("/tickets", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Vakantiedagen", jsonResponse.GetProperty("title").GetString());
        // Status wordt standaard op Open gezet, check dit in de JSON (0 is meestal de int-waarde voor de eerste Enum, pas evt. aan)
        Assert.Equal((int)TicketStatus.Open, jsonResponse.GetProperty("status").GetInt32()); 
    }

    [Fact]
    public async Task UpdateTicket_ReturnsOk_AndSuccessfullyAssignsUserAndChangesStatus()
    {
        // Arrange: Seed een complex scenario met creator, assignee en een origineel ticket
        Guid ticketId, assigneeId, departmentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dept = new Department("Tech", "Development");
            db.Departments.Add(dept);
            
            var creator = new User("Klant", "A", "klant@tech.com", "h", UserRole.Support, dept.Id);
            var assignee = new User("Dev", "B", "dev@tech.com", "h", UserRole.Admin, dept.Id);
            db.Users.AddRange(creator, assignee);
            await db.SaveChangesAsync();

            var ticket = new Ticket("Bug in prod", "Scherm knippert", TicketPriority.High, creator.Id, dept.Id);
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            ticketId = ticket.Id;
            assigneeId = assignee.Id;
            departmentId = dept.Id;
        }

        // We sturen een update waarbij we de status naar InProgress zetten en iemand assignen
        var updateRequest = new UpdateTicketRequest(
            "Bug in prod", 
            "Scherm knippert", 
            TicketStatus.InProgress, 
            TicketPriority.Critical, 
            assigneeId, 
            departmentId);

        // Act
        var response = await _client.PutAsJsonAsync($"/tickets/{ticketId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        // Verifieer de update in de teruggekomen JSON
        Assert.Equal((int)TicketStatus.InProgress, jsonResponse.GetProperty("status").GetInt32());
        Assert.Equal((int)TicketPriority.Critical, jsonResponse.GetProperty("priority").GetInt32());
        Assert.Equal(assigneeId.ToString(), jsonResponse.GetProperty("assignedToUserId").GetString());
    }

    [Fact]
    public async Task DeleteTicket_ReturnsNoContent_WhenTicketIsDeleted()
    {
        // Arrange: Seed een ticket om te verwijderen
        Guid ticketId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dept = new Department("TestDept", "Test");
            db.Departments.Add(dept);
            var user = new User("TestUser", "B", "test@test.com", "h", UserRole.Support, dept.Id);
            db.Users.Add(user);
            
            var ticket = new Ticket("Te verwijderen", "Dit mag weg", TicketPriority.Low, user.Id, dept.Id);
            db.Tickets.Add(ticket);
            
            await db.SaveChangesAsync();
            ticketId = ticket.Id;
        }

        // Act
        var response = await _client.DeleteAsync($"/tickets/{ticketId}");

        // Assert
        // Let op: afhankelijk van jouw API definitie is dit NoContent (204) of Ok (200)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); 
    }
}