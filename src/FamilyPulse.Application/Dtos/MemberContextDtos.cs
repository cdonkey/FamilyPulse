using FamilyPulse.Domain.Entities;

namespace FamilyPulse.Application.Contexts.DTOs;

public record MemberContextDto(
    Guid Id,
    Guid MemberId,
    Guid FamilyAccountId,
    int? Age,
    string? CommunicationPreference,
    List<string> Interests,
    List<string> TopicsToAvoid,
    List<AiMemoryItem> LearnedMemories,
    DateTime LastUpdatedAtUtc
);

public record UpdateMemberPreferencesCommand(
    int? Age,
    string? CommunicationPreference,
    List<string>? Interests,
    List<string>? TopicsToAvoid
);

public record RecordAiInsightCommand(
    string Category,
    string Fact,
    double Confidence
);