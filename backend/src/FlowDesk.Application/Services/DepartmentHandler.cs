// src/FlowDesk.Application/Services/DepartmentHandler.cs
using FlowDesk.Domain.Entities;
using FlowDesk.Application.Interfaces;

namespace FlowDesk.Application.Services;

public class DepartmentHandler
{
    private readonly IDepartmentRepository _repository;

    public DepartmentHandler(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Department> AddAsync(string name, string description)
    {
        var department = new Department(name, description);
        
        await _repository.AddAsync(department); 

        return department;
    }

    public async Task<Department?> UpdateAsync(Guid id, string name, string description)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department == null) return null;

        department.Update(name, description);
        await _repository.UpdateAsync(department);

        return department;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        var departments = await _repository.GetAllAsync();
        return departments;
    }
}