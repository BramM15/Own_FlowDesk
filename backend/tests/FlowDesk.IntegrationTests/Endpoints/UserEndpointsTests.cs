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

public class UserEndpointsTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;
    private readonly CustomApiFactory _factory;

    public UserEndpointsTests(CustomApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUser_ReturnsBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange: Seed een bestaande gebruiker in de DB
        Guid departmentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var department = new Department("Ops", "Operations");
            db.Departments.Add(department);
            
            // Deze email zit al in de database!
            var existingUser = new User("Dirk", "Vries", "dirk@ops.com", "hash", UserRole.Support, department.Id);
            db.Users.Add(existingUser);
            
            await db.SaveChangesAsync();
            departmentId = department.Id;
        }

        // We proberen een nieuwe user aan te maken met hetzelfde emailadres
        var request = new CreateUserRequest("Andere", "Naam", "dirk@ops.com", "Wachtwoord!", UserRole.Support, departmentId);

        // Act
        var response = await _client.PostAsJsonAsync("/users", request);

        // Assert
        // Let op: Afhankelijk van hoe jouw GlobalExceptionHandler is ingesteld, 
        // kan dit 400 BadRequest of 500 InternalServerError zijn. 
        // Best practice is dat een dubbele mail een 400 (Bad Request) of 409 (Conflict) is.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk_WithUpdatedRoleAndDepartment()
    {
        // Arrange: Seed de initiële staat
        Guid oldDepartmentId, newDepartmentId, userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dept1 = new Department("Oud", "Oud");
            var dept2 = new Department("Nieuw", "Nieuw");
            db.Departments.AddRange(dept1, dept2);
            
            var user = new User("Erik", "B", "erik@test.com", "hash", UserRole.Support, dept1.Id);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            oldDepartmentId = dept1.Id;
            newDepartmentId = dept2.Id;
            userId = user.Id;
        }

        // We updaten alleen Role en DepartmentId (zoals in jouw entiteit gedefinieerd)
        var updateRequest = new UpdateUserRequest(UserRole.Admin, newDepartmentId);

        // Act
        var response = await _client.PutAsJsonAsync($"/users/{userId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)UserRole.Admin, jsonResponse.GetProperty("role").GetInt32());
        Assert.Equal(newDepartmentId.ToString(), jsonResponse.GetProperty("departmentId").GetString());
    }
}