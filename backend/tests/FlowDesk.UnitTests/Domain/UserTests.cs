using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Xunit;

namespace FlowDesk.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesAndGenerateId()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@test.com";
        var passwordHash = "hashed_password_123";
        var role = (UserRole)1; // Gebruik een geldige enum waarde uit jouw UserRole
        var departmentId = Guid.NewGuid();

        // Act
        var user = new User(firstName, lastName, email, passwordHash, role, departmentId);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(firstName, user.FirstName);
        Assert.Equal(lastName, user.LastName);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(role, user.Role);
        Assert.Equal(departmentId, user.DepartmentId);
        
        // Controleer of de CreatedAt ongeveer 'nu' is ingesteld
        var timeDifference = DateTime.UtcNow - user.CreatedAt;
        Assert.True(timeDifference.TotalSeconds < 1);
    }

    [Fact]
    public void Update_ShouldChangeRoleAndDepartmentId()
    {
        // Arrange
        var user = new User("Jane", "Doe", "jane@test.com", "hash", (UserRole)1, Guid.NewGuid());
        var newRole = (UserRole)2;
        var newDepartmentId = Guid.NewGuid();

        // Act
        user.Update(newRole, newDepartmentId);

        // Assert
        Assert.Equal(newRole, user.Role);
        Assert.Equal(newDepartmentId, user.DepartmentId);
        Assert.Equal("Jane", user.FirstName); // Naam mag niet veranderd zijn
    }
}