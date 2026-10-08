using FamilyPulse.Application.Members.DTOs;

namespace FamilyPulse.Application.Common.Interfaces;

public interface IMemberService
{
    Task<List<MemberDto>> GetMembersAsync(Guid familyId, CancellationToken ct = default);
    Task<MemberDto?> AddMemberAsync(Guid familyId, AddMemberCommand command, CancellationToken ct = default);
    Task<MemberDto?> UpdateMemberAsync(Guid familyId, Guid memberId, UpdateMemberCommand command, CancellationToken ct = default);
    Task<bool> DeleteMemberAsync(Guid familyId, Guid memberId, CancellationToken ct = default);
}