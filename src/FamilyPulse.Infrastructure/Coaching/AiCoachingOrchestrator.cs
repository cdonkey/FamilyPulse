using System.Runtime.CompilerServices;
using System.Text;
using FamilyPulse.Application.Coaching;
using FamilyPulse.Application.Dtos;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyPulse.Infrastructure.Coaching;

public class AiCoachingOrchestrator : IAiCoachingOrchestrator
{
    private readonly AppDbContext _db;
    private readonly IChatClient? _chatClient;
    private readonly bool _isConfigured;
    private readonly ILogger<AiCoachingOrchestrator>? _logger;

    public AiCoachingOrchestrator(
        AppDbContext db,
        IConfiguration configuration,
        IChatClient? chatClient = null,
        ILogger<AiCoachingOrchestrator>? logger = null)
    {
        _db = db;
        _chatClient = chatClient;
        _logger = logger;

        var openAiKey = configuration["OpenAI:ApiKey"];
        var aiKey = configuration["AI:ApiKey"];
        var apiKey = openAiKey ?? aiKey;

        var keyPreview = !string.IsNullOrEmpty(apiKey) && apiKey.Length > 7
            ? $"{apiKey[..7]}... (len: {apiKey.Length})"
            : apiKey ?? "<NULL>";

        _logger?.LogInformation("=== [AiCoachingOrchestrator Diagnostics] ===");
        _logger?.LogInformation("  - Configuration['OpenAI:ApiKey']: {OpenAiKey}", openAiKey != null ? "FOUND" : "NULL");
        _logger?.LogInformation("  - Resolved Key Preview: {KeyPreview}", keyPreview);
        _logger?.LogInformation("  - IChatClient Injected: {HasChatClient} ({Type})", 
            _chatClient != null, 
            _chatClient?.GetType().Name ?? "null");

        _isConfigured = _chatClient != null 
                     && !string.IsNullOrWhiteSpace(apiKey) 
                     && apiKey != "key_placeholder" 
                     && apiKey != "mock-key";

        _logger?.LogInformation("  - Final _isConfigured: {IsConfigured}", _isConfigured);
        _logger?.LogInformation("===============================================");
    }

    public async IAsyncEnumerable<string> StreamCoachingAdviceAsync(
        Guid familyAccountId,
        Guid memberId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var domainSummaries = await AggregateDomainSummariesAsync(familyAccountId, memberId, ct);

        // Fallback to deterministic mock stream if unconfigured or IChatClient is null
        if (!_isConfigured || _chatClient == null)
        {
            await foreach (var token in StreamMockAdviceAsync(domainSummaries, ct))
            {
                yield return token;
            }
            yield break;
        }

        // Fetch recent check-ins for detailed qualitative prompt context
        var recentCheckIns = await _db.CheckIns
            .Where(c => c.FamilyAccountId == familyAccountId && c.MemberId == memberId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(5)
            .ToListAsync(ct);

        var systemPrompt = BuildSystemPrompt();
        var userTelemetryPrompt = BuildTelemetryUserPrompt(domainSummaries, recentCheckIns);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userTelemetryPrompt)
        };

        var options = new ChatOptions
        {
            Temperature = 0.3f,
            MaxOutputTokens = 1000
        };

        await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, options, ct))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }

    public IAsyncEnumerable<string> GenerateRecommendationAsync(
        Guid familyAccountId,
        Guid memberId,
        CancellationToken ct = default)
    {
        // Delegate to StreamCoachingAdviceAsync to keep logic DRY
        return StreamCoachingAdviceAsync(familyAccountId, memberId, ct);
    }

    private static string BuildSystemPrompt()
    {
        return """
            You are FamilyPulse AI, an expert systemic family therapist and organizational dynamics analyst.
            Your role is to analyze family telemetry data, identify stress drivers from contextual tags, 
            and output a structured, actionable harmony report.

            DOMAIN CONTEXT & SCALING RULES:
            - Domain Scale: -5.0 (High Tension/Crisis) to +5.0 (High Synergy/Flow). 0.0 is baseline neutral.
            - Focus on cross-domain interactions: Explain how high-performing domains can buffer low-scoring ones.
            - Search for root causes and ripple effects rather than treating domains in isolation.
            - Keep the tone compassionate, direct, and practical.

            OUTPUT FORMAT REQUIREMENTS:
            When summarizing metrics in a table, use warm, human-friendly column headers like 'Area | Current Level | What This Means' and avoid technical scale notation like '(-5 -> +5)'

            Always format your output in Markdown with these exact three sections:

            ### 1. Systemic Diagnosis
            Analyze overall equilibrium based on numerical scores (Stress, Closeness, Energy). In place of numerical output 3/4 
            1/5 -2/2 etc use rather words that match that score.
            

            ### 2. Multiplier Dynamics
            - **Primary Anchor:** Identify the highest-scoring domain and explain how to leverage it as an emotional stabilizer.
            - **Friction Vector:** Identify the lowest-scoring domain and describe how unresolved tension here ripples into other domains using contextual #tags and notes.

            ### 3. Weekly Micro-Habit
            Provide ONE concrete, low-friction, 3-minute actionable habit targeting the high-friction tags.
            """;
    }

    private static string BuildTelemetryUserPrompt(
        List<DomainHarmonySummaryDto> domainSummaries, 
        List<CheckIn> recentCheckIns)
    {
        if (recentCheckIns.Count == 0 && domainSummaries.Count == 0)
        {
            return "No check-in telemetry available. Provide a baseline family wellness recommendation.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Analyze the following family telemetry and recent check-in logs:");
        sb.AppendLine();

        // 1. High-level Domain Summaries
        if (domainSummaries.Count > 0)
        {
            sb.AppendLine("**Domain Harmony Averages:**");
            foreach (var summary in domainSummaries)
            {
                sb.AppendLine($"- Domain {summary.Category}: {summary.AverageScore:F2} score across {summary.TotalRatings} check-ins");
            }
            sb.AppendLine();
        }

        // 2. Aggregate Contextual Tags
        var allTags = recentCheckIns
            .Where(c => c.Tags != null)
            .SelectMany(c => c.Tags!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var avgStress = recentCheckIns.Count > 0 ? recentCheckIns.Average(c => c.StressLevel) : 0;
        var avgCloseness = recentCheckIns.Count > 0 ? recentCheckIns.Average(c => c.ClosenessLevel) : 0;
        var avgEnergy = recentCheckIns.Count > 0 ? recentCheckIns.Average(c => c.EnergyLevel) : 0;

        sb.AppendLine("**Recent Check-in Metrics (Last 5 Logs):**");
        sb.AppendLine($"- Average Scores: Stress = {avgStress:F1}/5 | Closeness = {avgCloseness:F1}/5 | Energy = {avgEnergy:F1}/5");
        sb.AppendLine($"- Contextual Tags: {(allTags.Count > 0 ? string.Join(", ", allTags) : "None recorded")}");
        sb.AppendLine();

        // 3. Detailed Check-in History
        sb.AppendLine("**Detailed Log Items:**");
        foreach (var checkIn in recentCheckIns)
        {
            var tagList = checkIn.Tags != null && checkIn.Tags.Count > 0
                ? string.Join(", ", checkIn.Tags)
                : "No tags";

            sb.AppendLine($"* **[{checkIn.CreatedAtUtc:yyyy-MM-dd}] Domain: {checkIn.Category} ({checkIn.Type})**");
            sb.AppendLine($"  - Scores: Stress={checkIn.StressLevel}/5, Closeness={checkIn.ClosenessLevel}/5, Energy={checkIn.EnergyLevel}/5");
            sb.AppendLine($"  - Tags: `{tagList}`");
            if (!string.IsNullOrWhiteSpace(checkIn.Note))
            {
                sb.AppendLine($"  - Note: \"{checkIn.Note}\"");
            }
        }

        return sb.ToString();
    }

    private async Task<List<DomainHarmonySummaryDto>> AggregateDomainSummariesAsync(
        Guid familyAccountId,
        Guid memberId,
        CancellationToken ct)
    {
        var checkIns = await _db.CheckIns
            .Where(c => c.FamilyAccountId == familyAccountId && c.MemberId == memberId)
            .ToListAsync(ct);

        return checkIns
            .GroupBy(c => c.Category)
            .Select(g => new DomainHarmonySummaryDto(
                Category: g.Key,
                AverageScore: g.Average(c => c.StressLevel - 3), // Scale shift: 1..5 -> -2.0..+2.0
                TotalRatings: g.Count()
            ))
            .ToList();
    }

    private static async IAsyncEnumerable<string> StreamMockAdviceAsync(
        List<DomainHarmonySummaryDto> domainSummaries,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var lowestDomain = domainSummaries.OrderBy(s => s.AverageScore).ThenBy(s => s.Category).FirstOrDefault();
        var highestDomain = domainSummaries.OrderByDescending(s => s.AverageScore).ThenBy(s => s.Category).FirstOrDefault();

        var highestCategory = highestDomain != null ? highestDomain.Category.ToString() : "General Communication";
        var lowestCategory = lowestDomain != null ? lowestDomain.Category.ToString() : "Stress Management";

        var highestScoreFormatted = highestDomain?.AverageScore.ToString("+0.00;-0.00;0.00") ?? "+0.00";
        var lowestScoreFormatted = lowestDomain?.AverageScore.ToString("+0.00;-0.00;0.00") ?? "-0.00";

        var mockText = $"""
            ### 1. Systemic Diagnosis
            [Mock AI Coaching]: Highest harmony observed in {highestCategory} ({highestScoreFormatted}). Focus next check-in on {lowestCategory} ({lowestScoreFormatted}), which shows elevated friction points.

            ### 2. Multiplier Dynamics
            - **Primary Anchor:** Leverage high stability in {highestCategory} as an emotional anchor.
            - **Friction Vector:** Address accumulated tension in {lowestCategory} before it spills into daily interactions.

            ### 3. Weekly Micro-Habit
            Schedule a 3-minute quiet appreciation check-in at the start of your next family meeting.
            """;

        foreach (var word in mockText.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(40, ct);
        }
    }
}