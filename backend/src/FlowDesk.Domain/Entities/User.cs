using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } 
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public Guid DepartmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Department Department { get; private set; } = default!;
    
    public User(string firstName, string lastName, string email, string passwordHash, UserRole role, Guid departmentId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        DepartmentId = departmentId;
    }
    
    public void ChangeRole(UserRole role)
    {
        Role = role;
    }

    public void ChangeDepartment(Guid departmentId)
    {
        DepartmentId = departmentId;
    }
}

