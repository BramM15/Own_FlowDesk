using FlowDesk.Domain.Enums;

namespace FlowDesk.API.DTOs;

public record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    Guid CreatedByUserId,
    Guid DepartmentId);

public record UpdateTicketRequest(
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid? AssignedToUserId,
    Guid DepartmentId);