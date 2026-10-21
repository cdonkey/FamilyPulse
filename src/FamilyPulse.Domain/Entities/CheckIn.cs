namespace FamilyPulse.Domain.Entities;

public enum CheckInType
{
    WeeklyRoutine = 0,
    LifeEvent = 1
}

public class CheckIn
{
    public Guid Id { get; private set; }
    public Guid FamilyAccountId { get; private set; }
    public Guid MemberId { get; private set; }
    public CheckInType Type { get; private set; }
    
    // Ratings on a 1-5 scale
    public int StressLevel { get; private set; }
    public int ClosenessLevel { get; private set; }
    public int EnergyLevel { get; private set; }
    
    public string? Note { get; private set; }
    public List<string> Tags { get; private set; } = new();
    public DateTime CreatedAtUtc { get; private set; }

    private CheckIn() { } // For EF Core

    public CheckIn(
        Guid familyAccountId, 
        Guid memberId, 
        CheckInType type, 
        int stressLevel, 
        int closenessLevel, 
        int energyLevel, 
        string? note = null, 
        List<string>? tags = null)
    {
        Id = Guid.NewGuid();
        FamilyAccountId = familyAccountId;
        MemberId = memberId;
        Type = type;
        StressLevel = Math.Clamp(stressLevel, 1, 5);
        ClosenessLevel = Math.Clamp(closenessLevel, 1, 5);
        EnergyLevel = Math.Clamp(energyLevel, 1, 5);
        Note = note?.Trim();
        Tags = tags ?? new List<string>();
        CreatedAtUtc = DateTime.UtcNow;
    }
}