using FamilyPulse.Domain.Enums;

namespace FamilyPulse.Application.Dtos;

public record MemberDto(Guid Id, string Name, string Role);

public record CreateRatingDto(Guid EvaluatorId, Guid RecipientId, DomainCategory Category, int Score, string? Note);

public record DomainHarmonySummaryDto(DomainCategory Category, double AverageScore, int TotalRatings);

public record AnnualHarmonyReportDto(
    Guid FamilyId,
    DateTime GeneratedAtUtc,
    List<DomainHarmonySummaryDto> DomainSummaries,
    string? AiCoachingAdvice
);