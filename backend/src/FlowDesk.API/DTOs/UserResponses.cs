using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.API.DTOs;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    Guid DepartmentId);
