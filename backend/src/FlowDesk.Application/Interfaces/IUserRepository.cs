using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetByDepartmentAsync(Guid departmentId);
    Task<User> AddAsync(User user);
    Task <User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}