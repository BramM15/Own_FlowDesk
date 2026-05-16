using FlowDesk.API.DTOs;
using FlowDesk.Application.Services;

namespace FlowDesk.API.Endpoints;

public static class DepartmentEndpoints
{
    public static void MapDepartmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/departments")
            .WithTags("Departments");

        group.MapPost("/", async (CreateDepartmentRequest request, DepartmentHandler handler) =>
            {
                var result = await handler.AddAsync(request.Name, request.Description);
                return Results.Ok(result);
            })
            .WithName("CreateDepartment");

        group.MapGet("/", async (DepartmentHandler handler) =>
            {
                var departments = await handler.GetAllAsync();
                return Results.Ok(departments);
            })
            .WithName("GetAllDepartments");

        group.MapPut("/{id}", async (Guid id, UpdateDepartmentRequest request, DepartmentHandler handler) =>
            {
                var updated = await handler.UpdateAsync(id, request.Name, request.Description);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            })
            .WithName("UpdateDepartment");

        group.MapDelete("/{id}", async (Guid id, DepartmentHandler handler) =>
            {
                var deleted = await handler.DeleteAsync(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteDepartment");
    }
}