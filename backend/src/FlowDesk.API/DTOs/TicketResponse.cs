using FlowDesk.Domain.Enums;

namespace FlowDesk.API.DTOs;

public record TicketResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    DateTime CreatedAt,
    DateTime? ClosedAt,
    Guid CreatedByUserId,
    string CreatedByUserName,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    Guid DepartmentId);