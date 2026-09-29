using FamilyPulse.Application.Dtos;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FamilyPulse.Application.Common.Interfaces;


namespace FamilyPulse.Application.Services;


public class HarmonyService
{
    private readonly IAppDbContext _db;
    

    public HarmonyService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DomainHarmonySummaryDto>> GetAnnualDomainSummariesAsync(CancellationToken cancellationToken = default)
    {
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);

        // Read-optimized LINQ query with AsNoTracking
        return await _db.Set<Rating>()
            .AsNoTracking()
            .Where(r => r.CreatedAtUtc >= oneYearAgo)
            .GroupBy(r => r.Category)
            .Select(g => new DomainHarmonySummaryDto(
                g.Key,
                Math.Round(g.Average(r => r.Score), 2),
                g.Count()
            ))
            .ToListAsync(cancellationToken);
    }
}