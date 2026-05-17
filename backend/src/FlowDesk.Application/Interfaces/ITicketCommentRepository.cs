using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Interfaces;

public interface ITicketCommentRepository
{
    Task<TicketComment?> GetAsync(Guid id);
    Task<List<TicketComment>> GetByTicketIdAsync(Guid ticketId);
    Task<TicketComment> AddAsync(TicketComment comment);
    Task DeleteAsync(Guid id);
}