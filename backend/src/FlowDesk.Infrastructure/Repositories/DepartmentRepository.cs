// src/FlowDesk.Infrastructure/Repositories/DepartmentRepository.cs
using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Department>> GetAllAsync()
    {
        return await _db.Departments.ToListAsync();
    }

    public async Task<Department> GetByIdAsync(Guid id)
    {
        return await _db.Departments.FindAsync(id);
    }

    public async Task UpdateAsync(Department department)
    {
        _db.Departments.Update(department);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null) return;
        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();
    }
}