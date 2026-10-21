using FamilyPulse.Application.Contexts.DTOs;

namespace FamilyPulse.Application.Common.Interfaces;

public interface IMemberContextService
{
    Task<MemberContextDto?> GetContextForMemberAsync(Guid familyId, Guid memberId, CancellationToken ct = default);
    Task<MemberContextDto> UpsertPreferencesAsync(Guid familyId, Guid memberId, UpdateMemberPreferencesCommand command, CancellationToken ct = default);
    Task<bool> AddAiMemoryAsync(Guid familyId, Guid memberId, RecordAiInsightCommand command, CancellationToken ct = default);
}