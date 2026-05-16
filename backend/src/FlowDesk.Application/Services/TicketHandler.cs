using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Services;

public class TicketHandler
{
    private readonly ITicketRepository _repository;

    public TicketHandler(ITicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<Ticket?> GetAsync(Guid id)
    {
        return await _repository.GetAsync(id);
    }

    public async Task<List<Ticket>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<List<Ticket>> GetByDepartmentAsync(Guid departmentId)
    {
        return await _repository.GetByDepartmentAsync(departmentId);
    }

    public async Task<List<Ticket>> GetByCreatedUserAsync(Guid userId)
    {
        return await  _repository.GetByCreatedUserAsync(userId);
    }
    
    public async Task<List<Ticket>> GetByAssignedUserAsync(Guid userId)
    {
        return await  _repository.GetByAssignedUserAsync(userId);
    }

    public async Task<Ticket> CreateAsync(
        string title,
        string description,
        TicketPriority priority,
        Guid createdByUserId,
        Guid departmentId)
    {
        var ticket = new Ticket(
            title,
            description,
            priority,
            createdByUserId,
            departmentId);

        return await _repository.AddAsync(ticket);
    }

    public async Task<Ticket> UpdateAsync(
        Guid id,
        string title,
        string description,
        TicketStatus status,
        TicketPriority priority,
        Guid? assignedToUserId,
        Guid departmentId)
    {
        var existingTicket = await _repository.GetAsync(id);

        if (existingTicket is null)
        {
            throw new Exception("Ticket not found");
        }

        existingTicket.Update(
            title,
            description,
            status,
            priority,
            assignedToUserId,
            departmentId);

        await _repository.UpdateAsync(existingTicket);

        return existingTicket;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existingTicket = await _repository.GetAsync(id);

        if (existingTicket is null)
        {
            throw new Exception("Ticket not found");
        }

        await _repository.DeleteAsync(id);
    }
}