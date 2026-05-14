using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services; // Jouw handler
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

app.MapPost("/departments", async (string name, string description, DepartmentHandler handler) =>
{
    var result = await handler.HandleAsync(name, description);
    return Results.Ok(result);
})
.WithName("CreateDepartment");

app.Run();