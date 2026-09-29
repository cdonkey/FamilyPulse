namespace FamilyPulse.Domain.Entities;

public class Member
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty; // e.g., "Parent", "Child"

    // Required for EF Core ORM materialization
    private Member() { }

    public Member(string name, string role)
    {
        Id = Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;

        Role = string.IsNullOrWhiteSpace(role) ? throw new ArgumentNullException(nameof(role)) : role;
    }
}

