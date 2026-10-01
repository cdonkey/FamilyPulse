using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Dtos;
using FamilyPulse.Application.Services;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Domain.Enums;
using FamilyPulse.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FamilyPulse.Application.Tests;

public class HarmonyServiceTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAnnualHarmonyReportAsync_ShouldCalculateCorrectDomainAverages_AndIgnoreOlderRatings()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var evaluatorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        // 1. Add ratings within the last year
        db.Ratings.AddRange(
            new Rating(evaluatorId, recipientId, DomainCategory.Marriage, 4, "Great communication"),
            new Rating(evaluatorId, recipientId, DomainCategory.Marriage, 2, "Minor disagreement"),
            new Rating(evaluatorId, recipientId, DomainCategory.Financial, -2, "Overspent on budget")
        );

        // 2. Add an old rating (>1 year ago) that should be filtered out by LINQ
        var oldRating = new Rating(evaluatorId, recipientId, DomainCategory.Financial, -5, "Ancient dispute");
        typeof(Rating).GetProperty(nameof(Rating.CreatedAtUtc))!
            .SetValue(oldRating, DateTime.UtcNow.AddYears(-2));
        db.Ratings.Add(oldRating);

        await db.SaveChangesAsync();

        // 3. Mock ICoachingService
        var mockCoachingService = new Mock<ICoachingService>();
        mockCoachingService
            .Setup(s => s.GenerateCoachingAdviceAsync(It.IsAny<List<DomainHarmonySummaryDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Mocked AI advice response");

        var service = new HarmonyService(db, mockCoachingService.Object);

        // Act
        var report = await service.GetAnnualHarmonyReportAsync();

        // Assert
        report.Should().NotBeNull();
        report.AiCoachingAdvice.Should().Be("Mocked AI advice response");
        report.DomainSummaries.Should().HaveCount(2);

        // Marriage domain check (Avg of +4 and +2 = +3.0)
        var marriageSummary = report.DomainSummaries.Single(s => s.Category == DomainCategory.Marriage);
        marriageSummary.AverageScore.Should().Be(3.0);
        marriageSummary.TotalRatings.Should().Be(2);

        // Financial domain check (Avg of -2, ignoring the 2-year-old -5 entry)
        var financialSummary = report.DomainSummaries.Single(s => s.Category == DomainCategory.Financial);
        financialSummary.AverageScore.Should().Be(-2.0);
        financialSummary.TotalRatings.Should().Be(1);

        // Verify coaching service was invoked once
        mockCoachingService.Verify(s => s.GenerateCoachingAdviceAsync(It.IsAny<List<DomainHarmonySummaryDto>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}