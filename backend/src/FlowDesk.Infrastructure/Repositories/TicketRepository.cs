using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _db;

    public TicketRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Ticket?> GetAsync(Guid id)
    {
        return await _db.Tickets.FindAsync(id);
    }

    public async Task<List<Ticket>> GetAllAsync()
    {
        return await _db.Tickets.ToListAsync();
    }

    public async Task<List<Ticket>> GetByDepartmentAsync(Guid departmentId)
    {
        return await _db.Tickets
            .Where(x => x.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetByCreatedUserAsync(Guid userId)
    {
        return await _db.Tickets
            .Where(x => x.CreatedByUserId == userId)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetByAssignedUserAsync(Guid userId)
    {
        return await _db.Tickets
            .Where(x => x.AssignedToUserId == userId)
            .ToListAsync();
    }

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        await _db.Tickets.AddAsync(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        _db.Tickets.Update(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    public async Task DeleteAsync(Guid id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null) return;

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();
    }
}