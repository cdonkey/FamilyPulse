using FamilyPulse.Application.Dtos;
using FamilyPulse.Application.Services;
using FamilyPulse.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace FamilyPulse.Application.Tests;

public class SemanticKernelCoachingServiceTests
{
    [Fact]
    public async Task GenerateCoachingAdviceAsync_WithoutApiKey_ReturnsFallbackMockMessage()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        var coachingService = new SemanticKernelCoachingService(emptyConfig);

        var summaries = new List<DomainHarmonySummaryDto>
        {
            new(DomainCategory.Marriage, 3.5, 10),
            new(DomainCategory.Financial, -1.2, 5)
        };

        // Act
        var result = await coachingService.GenerateCoachingAdviceAsync(summaries);
        

        // Assert
       Assert.StartsWith("[Mock AI Coaching]", result);
       Assert.Contains("Marriage", result);
       Assert.Contains("+3.50", result);
       Assert.Contains("Financial", result);
       Assert.Contains("-1.20", result);
    }
}