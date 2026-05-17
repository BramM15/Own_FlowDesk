using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Services;

public class TicketCommentHandler
{
    private readonly ITicketCommentRepository _repository;

    public TicketCommentHandler(ITicketCommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TicketComment>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _repository.GetByTicketIdAsync(ticketId);
    }

    public async Task<TicketComment> CreateAsync(string content, Guid ticketId, Guid userId)
    {
        var comment = new TicketComment(content, ticketId, userId);
        return await _repository.AddAsync(comment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await _repository.GetAsync(id);
        if (comment is null)
        {
            throw new Exception("Comment not found");
        }
        await _repository.DeleteAsync(id);
    }
}