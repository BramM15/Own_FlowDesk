using FlowDesk.Domain.Enums;

namespace FlowDesk.API.DTOs;

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string PasswordHash,
    UserRole Role,
    Guid DepartmentId);

public record UpdateUserRequest(
    UserRole Role,
    Guid DepartmentId);