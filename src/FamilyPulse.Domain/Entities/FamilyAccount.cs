namespace FamilyPulse.Domain.Entities;

public class FamilyAccount
{
    private readonly List<Member> _members = new();

    public Guid Id { get; private set; }
    public string IdentityHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastActiveAtUtc { get; private set; }

    public IReadOnlyCollection<Member> Members => _members.AsReadOnly();

    private FamilyAccount() { } // Required by EF Core

    public FamilyAccount(string identityHash)
    {
        if (string.IsNullOrWhiteSpace(identityHash))
            throw new ArgumentException("Identity hash is required.", nameof(identityHash));

        Id = Guid.NewGuid();
        IdentityHash = identityHash;
        CreatedAtUtc = DateTime.UtcNow;
        LastActiveAtUtc = DateTime.UtcNow;
    }


    public Member AddMember(string name, string role)
    {
        Member member = new Member(Id, name, role);
        _members.Add(member);
        return member;
    }

    public void RecordActivity()
    {
        LastActiveAtUtc = DateTime.UtcNow;
    }

    public bool IsInactive(TimeSpan threshold)
    {
        return DateTime.UtcNow - LastActiveAtUtc > threshold;
    }
}