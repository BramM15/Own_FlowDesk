using FlowDesk.API.DTOs;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Enums;

namespace FlowDesk.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users");

        group.MapGet("/", async (UserHandler handler) =>
            {
                var departments = await handler.GetAllAsync();
                return Results.Ok(departments);
            })
            .WithName("GetAllUsers");

        group.MapGet("/{id}", async (Guid id, UserHandler handler) =>
            {
                var user = await handler.GetAsync(id);
                return user is null ? Results.NotFound() : Results.Ok(user);
            })
            .WithName("GetUserById");

        group.MapGet("/department/{departmentId}", async (Guid departmentId, UserHandler handler) =>
            {
                var users = await handler.GetByDepartmentAsync(departmentId);
                return Results.Ok(users);
            })
            .WithName("GetUsersByDepartment");

        group.MapGet("/email/{email}", async (string email, UserHandler handler) =>
            {
                var user = await handler.GetByEmailAsync(email);
                return user is null ? Results.NotFound() : Results.Ok(user);
            })
            .WithName("GetUserByEmail");
        
        group.MapPost("/", async (CreateUserRequest request, UserHandler handler) =>
            {
                var newUser = await handler.CreateAsync(
                    request.FirstName, request.LastName, request.Email, request.PasswordHash, request.Role,
                    request.DepartmentId);
                return Results.Ok(newUser);
            })
            .WithName("CreateUser");

        group.MapPut("/{id}", async (Guid id, UpdateUserRequest request, UserHandler handler) =>
            {
                var updatedUser = await handler.UpdateAsync(id, request.Role, request.DepartmentId);
                return Results.Ok(updatedUser);
            })
            .WithName("UpdateUser");

        group.MapDelete("/{id:guid}", async (Guid id, UserHandler handler) =>
            {
                await handler.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteUser");
    }
}