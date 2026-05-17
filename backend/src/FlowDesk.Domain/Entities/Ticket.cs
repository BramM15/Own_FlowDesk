using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities;

public class Ticket
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public Guid DepartmentId { get; private set; }

    public User CreatedByUser { get; private set; } = default!;
    public User? AssignedToUser { get; private set; }
    public Department Department { get; private set; } = default!;

    public Ticket(
        string title, 
        string description, 
        TicketPriority priority, 
        Guid createdByUserId, 
        Guid departmentId)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Status = TicketStatus.Open;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        DepartmentId = departmentId;
    }

    private Ticket() { }

    public void Update(
        string title, 
        string description, 
        TicketStatus status, 
        TicketPriority priority, 
        Guid? assignedToUserId, 
        Guid departmentId)
    {
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        AssignedToUserId = assignedToUserId;
        DepartmentId = departmentId;
        UpdatedAt = DateTime.UtcNow;

        if (status == TicketStatus.Closed && ClosedAt is null)
        {
            ClosedAt = DateTime.UtcNow;
        }
        else if (status != TicketStatus.Closed)
        {
            ClosedAt = null;
        }
    }
}