using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Contexts.DTOs;
using FamilyPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Application.Contexts.Services;

public class MemberContextService : IMemberContextService
{
    private readonly IAppDbContext _dbContext;

    public MemberContextService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MemberContextDto?> GetContextForMemberAsync(
        Guid familyId, 
        Guid memberId, 
        CancellationToken ct = default)
    {
        var context = await _dbContext.MemberContexts
            .AsNoTracking()
            .FirstOrDefaultAsync(mc => mc.FamilyAccountId == familyId && mc.MemberId == memberId, ct);

        return context is null ? null : MapToDto(context);
    }

    public async Task<MemberContextDto> UpsertPreferencesAsync(
        Guid familyId, 
        Guid memberId, 
        UpdateMemberPreferencesCommand command, 
        CancellationToken ct = default)
    {
        var memberExists = await _dbContext.Members
            .AnyAsync(m => m.Id == memberId && m.FamilyAccountId == familyId, ct);

        if (!memberExists)
        {
            throw new KeyNotFoundException($"Member '{memberId}' was not found in family '{familyId}'.");
        }

        var context = await _dbContext.MemberContexts
            .FirstOrDefaultAsync(mc => mc.FamilyAccountId == familyId && mc.MemberId == memberId, ct);

        if (context is null)
        {
            context = new MemberContext(familyId, memberId, command.Age, command.CommunicationPreference);
            _dbContext.MemberContexts.Add(context);
        }

        context.UpdatePreferences(
            command.Age, 
            command.CommunicationPreference, 
            command.Interests, 
            command.TopicsToAvoid);

        await _dbContext.SaveChangesAsync(ct);

        return MapToDto(context);
    }

    public async Task<bool> AddAiMemoryAsync(
        Guid familyId, 
        Guid memberId, 
        RecordAiInsightCommand command, 
        CancellationToken ct = default)
    {
        var context = await _dbContext.MemberContexts
            .FirstOrDefaultAsync(mc => mc.FamilyAccountId == familyId && mc.MemberId == memberId, ct);

        if (context is null)
        {
            var memberExists = await _dbContext.Members
                .AnyAsync(m => m.Id == memberId && m.FamilyAccountId == familyId, ct);

            if (!memberExists) return false;

            context = new MemberContext(familyId, memberId);
            _dbContext.MemberContexts.Add(context);
        }

        context.AddOrUpdateAiMemory(command.Category, command.Fact, command.Confidence);
        await _dbContext.SaveChangesAsync(ct);

        return true;
    }

    private static MemberContextDto MapToDto(MemberContext context) =>
        new(
            context.Id,
            context.MemberId,
            context.FamilyAccountId,
            context.Age,
            context.CommunicationPreference,
            context.Interests,
            context.TopicsToAvoid,
            context.LearnedMemories,
            context.LastUpdatedAtUtc
        );
}