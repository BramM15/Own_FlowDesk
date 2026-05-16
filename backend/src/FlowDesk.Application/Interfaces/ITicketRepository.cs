using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetAsync(Guid id);
    Task<List<Ticket>> GetAllAsync();
    Task<List<Ticket>> GetByDepartmentAsync(Guid departmentId);
    Task<List<Ticket>> GetByCreatedUserAsync(Guid userId);
    Task<List<Ticket>> GetByAssignedUserAsync(Guid userId);
    Task<Ticket> AddAsync(Ticket ticket);
    Task<Ticket> UpdateAsync(Ticket ticket);
    Task DeleteAsync(Guid id);
}