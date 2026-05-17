using FlowDesk.API.DTOs;
using FlowDesk.Application.Services;

namespace FlowDesk.API.Endpoints;

public static class LoginEndpoint
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/login")
            .WithTags("Login");

        group.MapPost("/", async (LoginRequest request, AuthHandler handler) =>
            {
                var result = await handler.LoginAsync(request.Email, request.Password);
                return Results.Ok(result);
            })
            .WithName("Login");
    }
}