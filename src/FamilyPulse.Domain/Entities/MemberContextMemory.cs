namespace FamilyPulse.Domain.Entities;

public class MemberContextMemory
{
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    // Navigation property for EF Core
    public Member? Member { get; private set; }

    // Private constructor required by EF Core
    private MemberContextMemory() { }

    public MemberContextMemory(Guid memberId, string content, string category, string source)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Memory content cannot be empty.", nameof(content));

        Id = Guid.NewGuid();
        MemberId = memberId;
        Content = content;
        Category = category;
        Source = source;
        CreatedAtUtc = DateTime.UtcNow;
    }
}