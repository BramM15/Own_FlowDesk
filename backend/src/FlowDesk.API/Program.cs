using FlowDesk.Application.Interfaces;
using FlowDesk.Application.Services; // Jouw handler
using FlowDesk.Infrastructure.Database;
using FlowDesk.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FlowDesk.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<DepartmentHandler>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserHandler>();

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<TicketHandler>();

builder.Services.AddOpenApi(); // Voor Swagger/OpenAPI

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDepartmentEndpoints();
app.MapUserEndpoints();
app.MapTicketEndpoints();

app.Run();