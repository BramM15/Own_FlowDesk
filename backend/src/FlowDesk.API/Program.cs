using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services; // Jouw handler
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FlowDesk.API.DTOs;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

builder.Services.AddScoped<DepartmentHandler>();

builder.Services.AddOpenApi(); // Voor Swagger/OpenAPI

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

#region Routes

app.MapPost("/departments", async (CreateDepartmentRequest request, DepartmentHandler handler) =>
    {
        var result = await handler.AddAsync(request.Name, request.Description);
        return Results.Ok(result);
    })
    .WithName("CreateDepartment");

app.MapPut("/departments/{id}", async (Guid id, UpdateDepartmentRequest request, DepartmentHandler handler) =>
    {
        var updated = await handler.UpdateAsync(id, request.Name, request.Description);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    })
    .WithName("UpdateDepartment");

app.MapDelete("/departments/{id}", async (Guid id, DepartmentHandler handler) =>
    {
        var deleted = await handler.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    })
    .WithName("DeleteDepartment");

app.MapGet("/departments", async (DepartmentHandler handler) =>
    {
        var departments = await handler.GetAllAsync();
        return Results.Ok(departments);
    })
    .WithName("GetAllDepartments");

#endregion

app.Run();