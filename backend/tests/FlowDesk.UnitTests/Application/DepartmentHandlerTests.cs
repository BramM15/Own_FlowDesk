using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using Moq;

namespace FlowDesk.UnitTests.Application;

public class DepartmentHandlerTests
{
    private readonly DepartmentHandler _handler;
    private readonly Mock<IDepartmentRepository> _mockRepo;

    public DepartmentHandlerTests()
    {
        _mockRepo = new Mock<IDepartmentRepository>();
        _handler = new DepartmentHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateDepartment_AndCallRepository()
    {
        // Arrange
        var name = "Sales";
        var description = "Sales department";

        // Act
        var result = await _handler.AddAsync(name, description);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenDepartmentExists_ShouldUpdateAndReturnDepartment()
    {
        // Arrange
        var existingDepartment = new Department("Old Name", "Old Desc");
        _mockRepo.Setup(r => r.GetByIdAsync(existingDepartment.Id)).ReturnsAsync(existingDepartment);

        // Act
        var result = await _handler.UpdateAsync(existingDepartment.Id, "New Name", "New Desc");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        _mockRepo.Verify(r => r.UpdateAsync(existingDepartment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenDepartmentDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Department)null);

        // Act
        var result = await _handler.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}