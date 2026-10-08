using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Members.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Application.Members.Services;

public class MemberService : IMemberService
{
    private readonly IAppDbContext _dbContext;

    public MemberService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MemberDto>> GetMembersAsync(Guid familyId, CancellationToken ct = default)
    {
        return await _dbContext.Members
            .AsNoTracking()
            .Where(m => m.FamilyAccountId == familyId)
            .OrderBy(m => m.CreatedAtUtc)
            .Select(m => new MemberDto(m.Id, m.FamilyAccountId, m.Name, m.Role, (DateTime)m.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<MemberDto?> AddMemberAsync(Guid familyId, AddMemberCommand command, CancellationToken ct = default)
    {
        var familyAccount = await _dbContext.FamilyAccounts
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.Id == familyId, ct);

        if (familyAccount is null) return null;

        // Use Domain entity encapsulation
        var member = familyAccount.AddMember(command.Name, command.Role);


        _dbContext.Members.Add(member);
        
        await _dbContext.SaveChangesAsync(ct);

        return new MemberDto(
            member.Id, 
            member.FamilyAccountId, 
            member.Name, 
            member.Role, 
            (DateTime)member.CreatedAtUtc
        );
    }

    public async Task<MemberDto?> UpdateMemberAsync(Guid familyId, Guid memberId, UpdateMemberCommand command, CancellationToken ct = default)
    {
        var member = await _dbContext.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.FamilyAccountId == familyId, ct);

        if (member is null) return null;

        // Update domain entity properties
        member.Update(command.Name, command.Role);
        await _dbContext.SaveChangesAsync(ct);

        return new MemberDto(
            member.Id, 
            member.FamilyAccountId, 
            member.Name, 
            member.Role, 
            (DateTime)member.CreatedAtUtc
        );
    }

    public async Task<bool> DeleteMemberAsync(Guid familyId, Guid memberId, CancellationToken ct = default)
    {
        var member = await _dbContext.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.FamilyAccountId == familyId, ct);

        if (member is null) return false;

        _dbContext.Members.Remove(member);
        await _dbContext.SaveChangesAsync(ct);

        return true;
    }
}