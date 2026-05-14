// src/FlowDesk.Infrastructure/Database/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Department> Departments { get; set; }
}