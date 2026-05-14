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

    public async Task<Department> HandleAsync(string name, string description)
    {
        var department = new Department(name, description);
        
        await _repository.AddAsync(department); 

        return department;
    }
}