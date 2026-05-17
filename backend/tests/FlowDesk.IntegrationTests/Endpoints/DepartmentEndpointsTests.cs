using System.Net;
using System.Net.Http.Json;
using FlowDesk.API.DTOs;
using FlowDesk.Domain.Entities;
using FlowDesk.IntegrationTests.Setup;
using Xunit;

namespace FlowDesk.IntegrationTests.Endpoints;

public class DepartmentEndpointsTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;

    public DepartmentEndpointsTests(CustomApiFactory factory)
    {
        _client = factory.CreateClient(); // Maakt een HTTP Client aan voor de test API
    }

    [Fact]
    public async Task CreateDepartment_ReturnsOk_WithCreatedDepartment()
    {
        // Arrange
        var request = new CreateDepartmentRequest("Finance", "Geldzaken");

        // Act
        var response = await _client.PostAsJsonAsync("/departments", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var returnedDepartment = await response.Content.ReadFromJsonAsync<Department>();
        
        Assert.NotNull(returnedDepartment);
        Assert.Equal("Finance", returnedDepartment.Name);
        Assert.NotEqual(Guid.Empty, returnedDepartment.Id);
    }

    [Fact]
    public async Task GetAllDepartments_ReturnsOk_WithList()
    {
        // Arrange
        await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("IT", "Tech"));

        // Act
        var response = await _client.GetAsync("/departments");

        // Assert
        response.EnsureSuccessStatusCode();
        var departments = await response.Content.ReadFromJsonAsync<List<Department>>();
        Assert.NotNull(departments);
        Assert.NotEmpty(departments);
    }

    [Fact]
    public async Task DeleteDepartment_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/departments", new CreateDepartmentRequest("Operations", "Ops"));
        var department = await createResponse.Content.ReadFromJsonAsync<Department>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/departments/{department!.Id}");
        var getAgainResponse = await _client.GetAsync($"/departments"); 
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }
}