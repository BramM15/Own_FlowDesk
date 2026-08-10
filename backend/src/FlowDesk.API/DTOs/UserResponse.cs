using FlowDesk.Domain.Enums;

namespace FlowDesk.API.DTOs;

public record UserResponse(
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid DepartmentId);