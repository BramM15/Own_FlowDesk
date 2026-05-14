// src/FlowDesk.Application/Interfaces/IDepartmentRepository.cs
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Interfaces;

public interface IDepartmentRepository
{
    Task AddAsync(Department department);
}