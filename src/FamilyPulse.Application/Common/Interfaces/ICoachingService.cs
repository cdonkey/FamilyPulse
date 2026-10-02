using FamilyPulse.Application.Dtos;

namespace FamilyPulse.Application.Common.Interfaces;

public interface ICoachingService
{
    Task<string> GenerateCoachingAdviceAsync(
        List<DomainHarmonySummaryDto> domainSummaries, 
        CancellationToken ct = default);

    IAsyncEnumerable<string> StreamCoachingAdviceAsync(
        List<DomainHarmonySummaryDto> domainSummaries, 
        CancellationToken ct = default);
}