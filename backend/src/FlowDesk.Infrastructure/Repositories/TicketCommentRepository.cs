using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Repositories;

public class TicketCommentRepository : ITicketCommentRepository
{
    private readonly ApplicationDbContext _db;

    public TicketCommentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TicketComment?> GetAsync(Guid id)
    {
        return await _db.TicketComments.FindAsync(id);
    }

    public async Task<List<TicketComment>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _db.TicketComments
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<TicketComment> AddAsync(TicketComment comment)
    {
        await _db.TicketComments.AddAsync(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await _db.TicketComments.FindAsync(id);
        if (comment is null) return;

        _db.TicketComments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}