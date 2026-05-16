using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Services;

public class UserHandler
{
    private readonly IUserRepository _repository;
    
    public UserHandler(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<User?> GetAsync(Guid id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<List<User>> GetByDepartmentAsync(Guid departmentId)
    {
        return await _repository.GetByDepartmentAsync(departmentId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _repository.GetByEmailAsync(email);
    }

    public async Task<User> CreateAsync(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role,
        Guid departmentId)
    {
        var emailExists = await _repository.ExistsByEmailAsync(email);

        if (emailExists)
        {
            throw new Exception("Email already exists");
        }

        var user = new User(
            firstName,
            lastName,
            email,
            passwordHash,
            role,
            departmentId);

        return await _repository.AddAsync(user);
    }

    public async Task<User> UpdateAsync(Guid id, UserRole role, Guid departmentId)
    {
        var existingUser = await _repository.GetAsync(id);

        if (existingUser is null)
        {
            throw new Exception("User not found");
        }
        
        existingUser.Update(role, departmentId);

        await _repository.UpdateAsync(existingUser);
        
        return existingUser;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existingUser = await _repository.GetAsync(id);

        if (existingUser is null)
        {
            throw new Exception("User not found");
        }

        await _repository.DeleteAsync(id);
    }
    
}