namespace FlowDesk.Domain.Entities;

public class TicketComment
{
    public TicketComment(string content, Guid ticketId, Guid userId)
    {
        Id = Guid.NewGuid();
        Content = content;
        CreatedAt = DateTime.UtcNow;
        TicketId = ticketId;
        UserId = userId;
    }

    private TicketComment()
    {
    }

    public Guid Id { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; }

    public Ticket Ticket { get; private set; } = default!;
    public User User { get; private set; } = default!;
}