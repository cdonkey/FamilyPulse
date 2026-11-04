using FamilyPulse.Domain.Entities;
using FamilyPulse.Infrastructure.Coaching;
using FamilyPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using FamilyPulse.Domain.Enums;






namespace FamilyPulse.Infrastructure.Tests.Coaching;

public class AiCoachingOrchestratorTests
{


     private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
private static IConfiguration CreateTestConfig(string apiKey = "key_placeholder")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI:ApiKey"] = apiKey,
                ["OpenAI:ApiKey"] = apiKey,
                ["AI:ModelId"] = "gpt-4o-mini"
            })
            .Build();
    }

    [Fact]
    public async Task StreamCoachingAdviceAsync_WhenUnconfigured_ReturnsMockMarkdownStream()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var familyAccountId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        // Seed check-ins across domains
        db.CheckIns.AddRange(
            new CheckIn(familyAccountId, memberId, CheckInType.WeeklyRoutine, DomainCategory.Marriage, stressLevel: 5,closenessLevel: 3,energyLevel:3, note: "High stress"),
            new CheckIn(familyAccountId, memberId, CheckInType.WeeklyRoutine, DomainCategory.Financial, stressLevel: 1,closenessLevel: 5,energyLevel:5, note: "All good")
        );
        await db.SaveChangesAsync();



        var config = CreateTestConfig(apiKey: "key_placeholder");
        var orchestrator = new AiCoachingOrchestrator(db, config, chatClient: null);

        // Act
        var tokens = new List<string>();
        await foreach (var token in orchestrator.GenerateRecommendationAsync(familyAccountId, memberId))
        {
            tokens.Add(token);
        }

        var fullOutput = string.Join("", tokens);

        // Assert
        Assert.NotEmpty(tokens);
        Assert.Contains("### 1. Systemic Diagnosis", fullOutput);
        Assert.Contains("### 2. Multiplier Dynamics", fullOutput);
        Assert.Contains("### 3. Weekly Micro-Habit", fullOutput);
    }

    [Fact]
    public async Task StreamCoachingAdviceAsync_WhenNoCheckInsExist_HandlesEmptyDataGracefully()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var familyAccountId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var config = CreateTestConfig(apiKey: "sk-fake-test-key-12345");
        
        var orchestrator = new AiCoachingOrchestrator(db, config, chatClient: null);

        // Act
        var tokens = new List<string>();
        await foreach (var token in orchestrator.GenerateRecommendationAsync(familyAccountId, memberId))
        {
            tokens.Add(token);
        }

        var fullOutput = string.Join("", tokens);

        // Assert
        Assert.NotEmpty(tokens);
        Assert.Contains("Systemic Diagnosis", fullOutput);
    }
}