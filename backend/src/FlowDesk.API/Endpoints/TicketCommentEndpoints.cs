using FlowDesk.API.DTOs;
using FlowDesk.Application.Services;

namespace FlowDesk.API.Endpoints;

public static class TicketCommentEndpoints
{
    public static void MapTicketCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tickets/{ticketId:guid}/comments")
            .WithTags("TicketComments");

        group.MapGet("/", async (Guid ticketId, TicketCommentHandler handler) =>
            {
                var comments = await handler.GetByTicketIdAsync(ticketId);
                return Results.Ok(comments);
            })
            .WithName("GetCommentsByTicketId");

        group.MapPost("/", async (Guid ticketId, CreateCommentRequest request, TicketCommentHandler handler) =>
            {
                var comment = await handler.CreateAsync(request.Content, ticketId, request.UserId);
                return Results.Ok(comment);
            })
            .WithName("CreateTicketComment");

        app.MapDelete("/tickets/comments/{id}", async (Guid id, TicketCommentHandler handler) =>
            {
                await handler.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithTags("TicketComments")
            .WithName("DeleteTicketComment");
    }
}