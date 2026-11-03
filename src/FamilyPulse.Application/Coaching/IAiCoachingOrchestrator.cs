namespace FamilyPulse.Application.Coaching;

public record CoachingRecommendationDto(
    string Recommendation,
    string Rationale,
    List<string> SuggestedActivities,
    DateTime GeneratedAtUtc
);

public interface IAiCoachingOrchestrator
{


  public  IAsyncEnumerable<string> GenerateRecommendationAsync(
        Guid familyAccountId,
        Guid memberId,
       CancellationToken ct = default);

}