using FlowDesk.API.DTOs;
using FlowDesk.Application.Services;

namespace FlowDesk.API.Endpoints;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tickets")
            .WithTags("Tickets")
            .RequireAuthorization();

        group.MapGet("/", async (TicketHandler handler) =>
            {
                var tickets = await handler.GetAllAsync();

                var response = tickets.Select(t => new TicketResponse(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.CreatedAt,
                    t.ClosedAt,
                    t.CreatedByUserId,
                    $"{t.CreatedByUser.FirstName} {t.CreatedByUser.LastName}",
                    t.AssignedToUserId,
                    t.AssignedToUser is null
                        ? null
                        : $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}",
                    t.DepartmentId));

                return Results.Ok(response);
            })
            .WithName("GetAllTickets");

        group.MapGet("/{id}", async (Guid id, TicketHandler handler) =>
            {
                var ticket = await handler.GetAsync(id);
                return ticket is null ? Results.NotFound() : Results.Ok(ticket);
            })
            .WithName("GetTicketById");

        group.MapGet("/department/{departmentId}", async (Guid departmentId, TicketHandler handler) =>
            {
                var tickets = await handler.GetByDepartmentAsync(departmentId);
                return Results.Ok(tickets);
            })
            .WithName("GetTicketsByDepartment");

        group.MapGet("/createdby/{userId}", async (Guid userId, TicketHandler handler) =>
            {
                var tickets = await handler.GetByCreatedUserAsync(userId);
                return Results.Ok(tickets);
            })
            .WithName("GetTicketsByCreatedUser");

        group.MapGet("/assignedby/{userId}", async (Guid userId, TicketHandler handler) =>
            {
                var tickets = await handler.GetByAssignedUserAsync(userId);
                return Results.Ok(tickets);
            })
            .WithName("GetTicketsByAssignedUser");

        group.MapPost("/", async (CreateTicketRequest request, TicketHandler handler) =>
            {
                var newTicket = await handler.CreateAsync(
                    request.Title, request.Description, request.Priority, request.CreatedByUserId,
                    request.DepartmentId);
                return Results.Ok(newTicket);
            })
            .WithName("CreateTicket");

        group.MapPut("/{id}", async (Guid id, UpdateTicketRequest request, TicketHandler handler) =>
            {
                var updatedTicket = await handler.UpdateAsync(
                    id, request.Title, request.Description, request.Status, request.Priority,
                    request.AssignedToUserId, request.DepartmentId);
                return Results.Ok(updatedTicket);
            })
            .WithName("UpdateTicket");

        group.MapDelete("/{id}", async (Guid id, TicketHandler handler) =>
            {
                await handler.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteTicket");
    }
}