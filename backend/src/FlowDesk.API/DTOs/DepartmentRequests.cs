namespace FlowDesk.API.DTOs;

public record CreateDepartmentRequest(string Name, string Description);
public record UpdateDepartmentRequest(string Name, string Description);