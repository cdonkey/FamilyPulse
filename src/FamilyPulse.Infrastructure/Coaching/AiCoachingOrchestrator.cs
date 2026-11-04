using System.Runtime.CompilerServices;
using System.Text;
using FamilyPulse.Application.Coaching;
using FamilyPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using FamilyPulse.Application.Dtos;

namespace FamilyPulse.Infrastructure.Coaching;


public class AiCoachingOrchestrator : IAiCoachingOrchestrator
{
    private readonly AppDbContext _db;
    private readonly IChatClient? _chatClient;
    private readonly bool _isConfigured;

    public AiCoachingOrchestrator(AppDbContext db, IConfiguration configuration, IChatClient? chatClient = null)
    {
        _db = db;
        _chatClient = chatClient;

        var apiKey = configuration["OpenAI:ApiKey"] ?? configuration["AI:ApiKey"];
        _isConfigured = !string.IsNullOrWhiteSpace(apiKey) 
                     && apiKey != "key_placeholder" 
                     && apiKey != "mock-key" 
                     && _chatClient != null;
                     
    }

    private const string SystemPrompt = """
        You are FamilyPulse AI, an expert systemic family therapist and organizational dynamics analyst.
        Your task is to analyze family telemetry data and produce a structured, actionable harmony report.

        DOMAIN CONTEXT & SCALING RULES:
        - Scale: -5.0 (High Tension/Crisis) to +5.0 (High Synergy/Flow). 0.0 is baseline neutral.
        - Focus on cross-domain interaction: Explain how high-performing domains can buffer against low-scoring domains.
        - Do not treat domains in isolation; search for root causes and ripple effects.
        - Keep the tone compassionate, direct, and practical.
        """;










    public async IAsyncEnumerable<string> GenerateRecommendationAsync(
        Guid familyAccountId,
        Guid memberId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // 1. Fetch domain summaries / check-ins from database
        var domainSummaries = await AggregateDomainSummariesAsync(familyAccountId, memberId, ct);

        // 2. Fallback Mock Stream if AI is unconfigured
        if (!_isConfigured || _chatClient == null)
        {
            await foreach (var word in StreamMockAdviceAsync(domainSummaries, ct))
            {
                yield return word;
            }
            yield break;
        }

        // 3. Prepare Telemetry Payload for Prompt
        var telemetryText = FormatTelemetry(domainSummaries);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"""
                Analyze the following family telemetry summary:

                {telemetryText}

                Provide a structured coaching report strictly formatted in Markdown with these exact section headers:

                ### 1. Systemic Diagnosis
                A concise 2-sentence summary evaluating the family's current overall equilibrium.

                ### 2. Multiplier Dynamics
                - **Primary Anchor:** Identify the highest-scoring domain and explain how the family can leverage it as an emotional stabilizer.
                - **Friction Vector:** Identify the lowest-scoring domain and describe how unresolved tension here might spill over into other family domains.

                ### 3. Weekly Micro-Habit
                Provide ONE concrete, low-friction, non-confrontational action item or discussion prompt for this week's family meeting.
                """)
        };

        var options = new ChatOptions
        {
            Temperature = 0.3f,
            MaxOutputTokens = 600
        };

        // 4. Native Microsoft.Extensions.AI Streaming
        await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, options, ct))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
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
                AverageScore: g.Average(c => c.StressLevel - 3), // Normalizing to -2.0 .. +2.0 scale
                TotalRatings: g.Count()
            ))
            .ToList();
    }

    private static string FormatTelemetry(IEnumerable<DomainHarmonySummaryDto> summaries)
    {
        var sb = new StringBuilder();
        foreach (var summary in summaries)
        {
            sb.AppendLine($"Domain: {summary.Category}");
            sb.AppendLine($"  Average Harmony Score: {summary.AverageScore:F2} (Scale: -5.0 High Friction to +5.0 High Harmony)");
            sb.AppendLine($"  Logged Check-ins: {summary.TotalRatings}");
        }
        return sb.ToString();
    }

    private static async IAsyncEnumerable<string> StreamMockAdviceAsync(
        List<DomainHarmonySummaryDto> domainSummaries,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var lowestDomain = domainSummaries.OrderBy(s => s.AverageScore).ThenBy(s => s.Category).FirstOrDefault();
        var highestDomain = domainSummaries.OrderByDescending(s => s.AverageScore).ThenBy(s => s.Category).FirstOrDefault();

        var highestScoreFormatted = highestDomain?.AverageScore.ToString("+0.00;-0.00;0.00") ?? "+0.00";
        var lowestScoreFormatted = lowestDomain?.AverageScore.ToString("+0.00;-0.00;0.00") ?? "-0.00";

         

        var mockText = $"### 1. Systemic Diagnosis\n[Mock AI Coaching]: Highest harmony observed in {highestDomain?.Category } ({highestScoreFormatted}). Focus next check-in on {lowestDomain?.Category } ({lowestScoreFormatted}), which shows elevated friction points.\n\n### 2. Multiplier Dynamics\n- **Primary Anchor:** Leverage high stability in {highestDomain?.Category}.\n- **Friction Vector:** Address friction in {lowestDomain?.Category}.\n\n### 3. Weekly Micro-Habit\nSchedule a 10-minute quiet check-in with your family.";

        foreach (var word in mockText.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(40, ct);
        }
    }
}