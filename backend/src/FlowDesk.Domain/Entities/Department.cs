namespace FlowDesk.Domain.Entities;

public class Department
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    
    public Department(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}