using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Moq;
using Xunit;

namespace FlowDesk.UnitTests.Application;

public class UserHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IPasswordHasher> _mockHasher;
    private readonly UserHandler _handler;

    public UserHandlerTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockHasher = new Mock<IPasswordHasher>();
        _handler = new UserHandler(_mockRepo.Object, _mockHasher.Object);
    }

    // --- GET METHODS ---

    [Fact]
    public async Task GetAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User("Test", "User", "test@test.com", "hash", default, Guid.NewGuid());
        _mockRepo.Setup(r => r.GetAsync(userId)).ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.GetAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnListOfUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User("A", "A", "a@a.com", "hash", default, Guid.NewGuid()),
            new User("B", "B", "b@b.com", "hash", default, Guid.NewGuid())
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _handler.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByDepartmentAsync_ShouldReturnUsersForDepartment()
    {
        // Arrange
        var deptId = Guid.NewGuid();
        var users = new List<User> { new User("A", "A", "a@a.com", "hash", default, deptId) };
        _mockRepo.Setup(r => r.GetByDepartmentAsync(deptId)).ReturnsAsync(users);

        // Act
        var result = await _handler.GetByDepartmentAsync(deptId);

        // Assert
        Assert.Single(result);
        Assert.Equal(deptId, result.First().DepartmentId);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser()
    {
        // Arrange
        var email = "info@flowdesk.com";
        var expectedUser = new User("Flow", "Desk", email, "hash", default, Guid.NewGuid());
        _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    // --- CREATE METHODS ---

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "taken@test.com";
        _mockRepo.Setup(r => r.ExistsByEmailAsync(email)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.CreateAsync("Test", "User", email, "password123", default, Guid.NewGuid()));

        Assert.Equal("Email already exists", exception.Message);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never); // Zorg dat AddAsync NOOIT is aangeroepen
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPasswordAndSaveUser_WhenEmailIsUnique()
    {
        // Arrange
        var email = "new@test.com";
        var plainPassword = "SecretPassword!";
        var hashedPassword = "HashedSecretPassword!";
        
        _mockRepo.Setup(r => r.ExistsByEmailAsync(email)).ReturnsAsync(false);
        _mockHasher.Setup(h => h.Hash(plainPassword)).Returns(hashedPassword);

        // We laten de repository de gesavede user teruggeven (zoals het hoort in je applicatie)
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
                 .ReturnsAsync((User u) => u);

        // Act
        var result = await _handler.CreateAsync("Test", "User", email, plainPassword, default, Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(hashedPassword, result.PasswordHash); // Controleer of het GEHASHDE wachtwoord in de entiteit zit
        
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    // --- UPDATE METHODS ---

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.UpdateAsync(userId, default, Guid.NewGuid()));

        Assert.Equal("User not found", exception.Message);
        _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndSave_WhenUserExists()
    {
        // Arrange
        var existingUser = new User("Test", "User", "test@test.com", "hash", (UserRole)1, Guid.NewGuid());
        var newRole = (UserRole)2;
        var newDeptId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAsync(existingUser.Id)).ReturnsAsync(existingUser);

        // Act
        var result = await _handler.UpdateAsync(existingUser.Id, newRole, newDeptId);

        // Assert
        Assert.Equal(newRole, result.Role);
        Assert.Equal(newDeptId, result.DepartmentId);
        _mockRepo.Verify(r => r.UpdateAsync(existingUser), Times.Once);
    }

    // --- DELETE METHODS ---

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.DeleteAsync(userId));

        Assert.Equal("User not found", exception.Message);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelete_WhenUserExists()
    {
        // Arrange
        var existingUser = new User("Test", "User", "test@test.com", "hash", default, Guid.NewGuid());
        _mockRepo.Setup(r => r.GetAsync(existingUser.Id)).ReturnsAsync(existingUser);

        // Act
        await _handler.DeleteAsync(existingUser.Id);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(existingUser.Id), Times.Once);
    }
}