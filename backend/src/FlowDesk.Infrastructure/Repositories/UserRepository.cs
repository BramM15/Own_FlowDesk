// src/FlowDesk.Infrastructure/Repositories/UserRepository.cs

using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FlowDesk.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetAsync(Guid id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _db.Users
            .AnyAsync(x => x.Email == email);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task<List<User>> GetAllByRoleAsync(UserRole role)
    {
        return await _db.Users
            .Where(x => x.Role == role)
            .ToListAsync();
    }

    public async Task<List<User>> GetByDepartmentAsync(Guid departmentId)
    {
        return await _db.Users
            .Where(x => x.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await _db.Users.AddAsync(user);

        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _db.Users.Update(user);

        await _db.SaveChangesAsync();

        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null) return;

        _db.Users.Remove(user);

        await _db.SaveChangesAsync();
    }
}