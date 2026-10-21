namespace FamilyPulse.Domain.Entities;

public record AiMemoryItem(
    string Category,      // e.g., "Stress Trigger", "Interest", "Communication Style"
    string Fact,          // e.g., "Gets overwhelmed by sudden routine changes"
    double Confidence,    // 0.0 to 1.0 rating from LLM
    DateTime ObservedAtUtc
);

public class MemberContext
{
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public Guid FamilyAccountId { get; private set; }

    // Demographics & Preferences
    public int? Age { get; private set; }
    public string? CommunicationPreference { get; private set; } // e.g., "Gentle", "Direct", "Gamified"
    public List<string> Interests { get; private set; } = new();
    public List<string> TopicsToAvoid { get; private set; } = new();

    // AI-Learned Memory Vault (Dynamic insights extracted by Grok)
    public List<AiMemoryItem> LearnedMemories { get; private set; } = new();

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }

    private MemberContext() { } // For EF Core

    public MemberContext(Guid familyAccountId, Guid memberId, int? age = null, string? communicationPreference = null)
    {
        Id = Guid.NewGuid();
        FamilyAccountId = familyAccountId;
        MemberId = memberId;
        Age = age;
        CommunicationPreference = communicationPreference?.Trim();
        CreatedAtUtc = DateTime.UtcNow;
        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePreferences(int? age, string? communicationPreference, List<string>? interests, List<string>? topicsToAvoid)
    {
        if (age.HasValue) Age = age.Value;
        if (communicationPreference != null) CommunicationPreference = communicationPreference.Trim();
        if (interests != null) Interests = interests;
        if (topicsToAvoid != null) TopicsToAvoid = topicsToAvoid;

        LastUpdatedAtUtc = DateTime.UtcNow;
    }

    // Encapsulated behavior for AI memory updates
    public void AddOrUpdateAiMemory(string category, string fact, double confidence)
    {
        // Replace existing fact in the same category if similar, otherwise append
        var existing = LearnedMemories.FirstOrDefault(m => 
            m.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && 
            m.Fact.Equals(fact, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            LearnedMemories.Remove(existing);
        }

        LearnedMemories.Add(new AiMemoryItem(
            Category: category.Trim(),
            Fact: fact.Trim(),
            Confidence: Math.Clamp(confidence, 0.0, 1.0),
            ObservedAtUtc: DateTime.UtcNow
        ));

        LastUpdatedAtUtc = DateTime.UtcNow;
    }
}