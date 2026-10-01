using FamilyPulse.Application.Dtos;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FamilyPulse.Application.Common.Interfaces;


namespace FamilyPulse.Application.Services;


public class HarmonyService
{
    private readonly IAppDbContext _db;
    private readonly ICoachingService _coachingService;
    

    public HarmonyService(IAppDbContext db, ICoachingService coachingService)
    {
        _db = db;
        _coachingService = coachingService;
    }

public async Task<AnnualHarmonyReportDto> GetAnnualHarmonyReportAsync(CancellationToken cancellationToken = default)
    {
        var oneYearAgo = DateTime.UtcNow.AddYears(-1);

        var domainSummaries = await _db.Ratings
            .AsNoTracking()
            .Where(r => r.CreatedAtUtc >= oneYearAgo)
            .GroupBy(r => r.Category)
            .Select(g => new DomainHarmonySummaryDto(
                g.Key,
                Math.Round(g.Average(r => r.Score), 2),
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        var aiAdvice = await _coachingService.GenerateCoachingAdviceAsync(domainSummaries, cancellationToken);

        return new AnnualHarmonyReportDto(
            FamilyId: Guid.NewGuid(),
            GeneratedAtUtc: DateTime.UtcNow,
            DomainSummaries: domainSummaries,
            AiCoachingAdvice: aiAdvice
        );
    }
}