using FamilyPulse.Application.CheckIns.Dtos;

using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Application.CheckIns.Services;

public class CheckInService : ICheckInService
{
    private readonly IAppDbContext _dbContext;

    public CheckInService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CheckInDto?> CreateCheckInAsync(Guid familyId, CreateCheckInCommand command, CancellationToken ct = default)
    {
        // Verify member exists within family aggregate
        var memberExists = await _dbContext.Members
            .AnyAsync(m => m.Id == command.MemberId && m.FamilyAccountId == familyId, ct);

        if (!memberExists) return null;

        var checkIn = new CheckIn(
            familyAccountId: familyId,
            memberId: command.MemberId,
            type: command.Type,
            stressLevel: command.StressLevel,
            closenessLevel: command.ClosenessLevel,
            energyLevel: command.EnergyLevel,
            note: command.Note,
            tags: command.Tags
        );

        _dbContext.CheckIns.Add(checkIn);
        await _dbContext.SaveChangesAsync(ct);

        return new CheckInDto(
            checkIn.Id,
            checkIn.FamilyAccountId,
            checkIn.MemberId,
            checkIn.Type,
            checkIn.StressLevel,
            checkIn.ClosenessLevel,
            checkIn.EnergyLevel,
            checkIn.Note,
            checkIn.Tags,
            checkIn.CreatedAtUtc
        );
    }

    public async Task<List<CheckInDto>> GetCheckInsForFamilyAsync(Guid familyId, int limit = 20, CancellationToken ct = default)
    {
        return await _dbContext.CheckIns
            .AsNoTracking()
            .Where(c => c.FamilyAccountId == familyId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(limit)
            .Select(c => new CheckInDto(
                c.Id,
                c.FamilyAccountId,
                c.MemberId,
                c.Type,
                c.StressLevel,
                c.ClosenessLevel,
                c.EnergyLevel,
                c.Note,
                c.Tags,
                c.CreatedAtUtc))
            .ToListAsync(ct);
    }
}