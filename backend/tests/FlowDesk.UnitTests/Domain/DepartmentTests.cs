using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain;

public class DepartmentTests
{
    [Fact]
    public void Constructor_ShouldSetPropertiesAndGenerateId()
    {
        // Arrange
        var name = "IT Support";
        var description = "Beheert alle IT gerelateerde zaken";

        // Act
        var department = new Department(name, description);

        // Assert
        Assert.NotEqual(Guid.Empty, department.Id);
        Assert.Equal(name, department.Name);
        Assert.Equal(description, department.Description);
    }

    [Fact]
    public void Update_ShouldChangeProperties()
    {
        // Arrange
        var department = new Department("HR", "Human Resources");
        var newName = "HR & Payroll";
        var newDescription = "Beheert HR en salarisadministratie";

        // Act
        department.Update(newName, newDescription);

        // Assert
        Assert.Equal(newName, department.Name);
        Assert.Equal(newDescription, department.Description);
    }
}