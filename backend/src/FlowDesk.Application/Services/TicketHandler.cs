using System.Security.Claims;
using FlowDesk.Application.Interfaces;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FlowDesk.Application.Services;

public class TicketHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITicketRepository _repository;

    public TicketHandler(ITicketRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
    }
    
    private (Guid UserId, string Role, Guid DepartmentId)? GetCurrentUserContext()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity!.IsAuthenticated) return null;

        Console.WriteLine(user);
        
        var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleStr = user.FindFirst(ClaimTypes.Role)?.Value;
        var departmentIdStr = user.FindFirst("DepartmentId")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(roleStr) || string.IsNullOrEmpty(departmentIdStr) || !Guid.TryParse(userIdStr, out Guid userId) || !Guid.TryParse(departmentIdStr, out Guid userDepartmentId))
        {
            return null;
        }

        return (userId, roleStr, userDepartmentId);
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
        var context = GetCurrentUserContext();
        
        if (context == null)
        {
            throw new UnauthorizedAccessException("Gebruiker is niet ingelogd.");
        }
        
        if  (!context.Value.DepartmentId.Equals(departmentId) || context.Value.Role is "User"){
            throw new UnauthorizedAccessException("Gebruiker is unauthenticated.");
        }
        
        var allTickets = await _repository.GetByDepartmentAsync(departmentId);;
        
        return allTickets;
    }

    public async Task<List<Ticket>> GetByCreatedUserAsync(Guid userId)
    {
        return await _repository.GetByCreatedUserAsync(userId);
    }

    public async Task<List<Ticket>> GetByAssignedUserAsync(Guid userId)
    {
        return await _repository.GetByAssignedUserAsync(userId);
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

        if (existingTicket is null) throw new Exception("Ticket not found");

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

        if (existingTicket is null) throw new Exception("Ticket not found");

        await _repository.DeleteAsync(id);
    }
}