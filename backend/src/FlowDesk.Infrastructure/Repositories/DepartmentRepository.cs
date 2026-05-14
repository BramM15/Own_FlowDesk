// src/FlowDesk.Infrastructure/Repositories/DepartmentRepository.cs
using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Database;

namespace FlowDesk.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _db;

    public DepartmentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Department department)
    {
        await _db.Departments.AddAsync(department);
        await _db.SaveChangesAsync(); 
    }
}