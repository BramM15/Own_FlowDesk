using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.IntegrationTests.Repositories;

public class DepartmentRepositoryTests : IClassFixture<CustomApiFactory>
{
    private readonly CustomApiFactory _factory;

    public DepartmentRepositoryTests(CustomApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddAsync_ShouldSaveDepartmentToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new DepartmentRepository(dbContext);

        var department = new Department("Marketing", "Marketing afdeling");

        // Act
        await repository.AddAsync(department);

        // Assert
        var dbDepartment = await dbContext.Departments.FindAsync(department.Id);
        Assert.NotNull(dbDepartment);
        Assert.Equal("Marketing", dbDepartment.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveDepartmentFromDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new DepartmentRepository(dbContext);

        var department = new Department("To Delete", "Zal worden verwijderd");
        await dbContext.Departments.AddAsync(department);
        await dbContext.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(department.Id);

        // Assert
        var deletedDept = await dbContext.Departments.FindAsync(department.Id);
        Assert.Null(deletedDept);
    }
}