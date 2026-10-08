namespace FamilyPulse.Domain.Entities;

public class Member
{
    public Guid Id { get; private set; }
    public Guid FamilyAccountId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty; // e.g., "Parent", "Child"
    public DateTime CreatedAtUtc { get; set; }


    // Required for EF Core ORM materialization
    private Member() { }

    public Member(Guid familyAccountId, string name, string role)
    {
        if (familyAccountId == Guid.Empty)
            throw new ArgumentException("Family account ID is required.", nameof(familyAccountId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Id = Guid.NewGuid();
        FamilyAccountId = familyAccountId;
        Name = name;
        Role = role;
    }


    public void Update(string name, string role)
{
    Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
    Role = string.IsNullOrWhiteSpace(role) ? Role : role.Trim();
}
}

