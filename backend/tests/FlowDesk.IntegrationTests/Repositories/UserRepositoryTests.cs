using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using FlowDesk.IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.IntegrationTests.Repositories;

public class UserRepositoryTests : IClassFixture<CustomApiFactory>
{
    private readonly CustomApiFactory _factory;

    public UserRepositoryTests(CustomApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddAsync_ShouldSaveUserToDatabase()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new UserRepository(dbContext);

        // 1. Maak eerst een department aan vanwege de Foreign Key relatie
        var department = new Department("IT", "IT Afdeling");
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync();

        // 2. Maak de test user aan
        var user = new User("Anna", "Jansen", "anna@test.com", "hash123", UserRole.Support, department.Id);

        // Act
        await repository.AddAsync(user);

        // Assert
        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == "anna@test.com");
        Assert.NotNull(dbUser);
        Assert.Equal("Anna", dbUser.FirstName);
        Assert.Equal(department.Id, dbUser.DepartmentId);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repository = new UserRepository(dbContext);

        var department = new Department("HR", "HR Afdeling");
        dbContext.Departments.Add(department);
        var user = new User("Bob", "Smit", "bob@test.com", "hash", UserRole.Admin, department.Id);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var exists = await repository.ExistsByEmailAsync("bob@test.com");
        var doesNotExist = await repository.ExistsByEmailAsync("niemand@test.com");

        // Assert
        Assert.True(exists);
        Assert.False(doesNotExist);
    }
}