using FamilyPulse.Domain.Enums;

namespace FamilyPulse.Domain.Entities;

public class Rating
{
    public Guid Id { get; private set; }
    public Guid EvaluatorId { get; private set; }
    public Guid RecipientId { get; private set; }
    public DomainCategory Category { get; private set; }
    public int Score { get; private set; } // Enforced -5 (high friction) to +5 (high harmony)
    public string? Note { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Rating() { }

    public Rating(Guid evaluatorId, 
                  Guid recipientId, 
                  DomainCategory category,
                  int score,
                  string? note = null,
                  DateTime? createdAtUtc = null)
    {
        if (score is < -5 or > 5)
            throw new ArgumentOutOfRangeException(nameof(score), "Score must be between -5 and +5.");

        Id = Guid.NewGuid();
        EvaluatorId = evaluatorId;
        RecipientId = recipientId;
        Category = category;
        Score = score;
        Note = note;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}