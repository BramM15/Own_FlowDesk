// src/FlowDesk.Application/Interfaces/IDepartmentRepository.cs
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Interfaces;

public interface IDepartmentRepository
{
    Task AddAsync(Department department);
    Task<List<Department>> GetAllAsync();
    Task<Department> GetByIdAsync(Guid id);
    Task UpdateAsync(Department department);
    Task DeleteAsync(Guid id);
}