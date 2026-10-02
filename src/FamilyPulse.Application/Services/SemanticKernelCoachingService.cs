using System.Text;
using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace FamilyPulse.Application.Services;

public class SemanticKernelCoachingService : ICoachingService
{
    private readonly Kernel? _kernel;
    private readonly bool _isConfigured;

    public String PromptTemplate()
    {
       return  """
            <message role="system">
            You are FamilyPulse AI, an expert systemic family therapist and organizational dynamics analyst.
            Your task is to analyze 12-month family telemetry data and produce a structured, actionable harmony report.

            DOMAIN CONTEXT & SCALING RULES:
            - Scale: -5.0 (High Tension/Crisis) to +5.0 (High Synergy/Flow). 0.0 is baseline neutral.
            - Focus on cross-domain interaction: Explain how high-performing domains can buffer against low-scoring domains.
            - Do not treat domains in isolation; search for root causes and ripple effects.
            - Keep the tone compassionate, direct, and practical.
            </message>

            <message role="user">
            Analyze the following 12-month family telemetry summary:

            {{$telemetryData}}

            Provide a structured coaching report strictly formatted in Markdown with these exact section headers:

            ### 1. Systemic Diagnosis
            A concise 2-sentence summary evaluating the family's current overall equilibrium.

            ### 2. Multiplier Dynamics
            - **Primary Anchor:** Identify the highest-scoring domain and explain how the family can leverage it as a emotional stabilizer.
            - **Friction Vector:** Identify the lowest-scoring domain and describe how unresolved tension here might spill over into other family domains.

            ### 3. Weekly Micro-Habit
            Provide ONE concrete, low-friction, non-confrontational action item or discussion prompt for this week's family meeting.
            </message>
            """;
    }


public SemanticKernelCoachingService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]  ?? "key_placeholder";
        var modelId = configuration["OpenAI:ModelId"] ?? "llama-3.3-70b-versatile";
        var openAiEndpoint = configuration["OpenAI:Endpoint"];

        // 1. Check if a valid API key exists (ignoring placeholders)
        _isConfigured = !string.IsNullOrWhiteSpace(apiKey) && apiKey != "key_placeholder";

        if (_isConfigured)
        {
            var kernelBuilder = Kernel.CreateBuilder();

            if (!string.IsNullOrWhiteSpace(openAiEndpoint) && Uri.TryCreate(openAiEndpoint, UriKind.Absolute, out var endpointUri))
            {
                // Custom provider (Groq / OpenRouter / Local Ollama)
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey!,
                    endpoint: endpointUri
                );
            }
            else
            {
                // Direct OpenAI
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey!
                );
            }

            // 2. Build and store the Kernel instance
            _kernel = kernelBuilder.Build();
        }
    }

  

public async IAsyncEnumerable<string> StreamCoachingAdviceAsync(
        List<DomainHarmonySummaryDto> domainSummaries,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // 1. Mock Fallback Mode (Simulates SSE stream word by word)
        if (!_isConfigured || _kernel == null)
        {
            var lowestDomain = domainSummaries.OrderBy(s => s.AverageScore).ThenBy(s => s.Category.ToString()).FirstOrDefault();
            var highestDomain = domainSummaries.OrderByDescending(s => s.AverageScore).ThenBy(s => s.Category.ToString()).FirstOrDefault();

            var highestScoreFormatted = highestDomain?.AverageScore.ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) ?? "+0.00";
            var lowestScoreFormatted = lowestDomain?.AverageScore.ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) ?? "-0.00";

            var mockText = $"[Mock AI Coaching]: Highest harmony observed in {highestDomain?.Category} ({highestScoreFormatted}). Focus next check-in on {lowestDomain?.Category} ({lowestScoreFormatted}), which shows elevated friction points.";

            foreach (var word in mockText.Split(' '))
            {
                ct.ThrowIfCancellationRequested();
                yield return word + " ";
                await Task.Delay(50, ct); // Simulated network/LLM latency
            }
            yield break;
        }

        // 2. Real OpenAI / Semantic Kernel Stream
   
        // 1. Format raw telemetry with contextual descriptions
        var summaryText = new StringBuilder();
        foreach (var summary in domainSummaries)
        {
            summaryText.AppendLine($"- Domain: {summary.Category}");
            summaryText.AppendLine($"  Average Harmony Score: {summary.AverageScore:F2} (Scale: -5.0 High Friction to +5.0 High Harmony)");
            summaryText.AppendLine($"  Logged Check-ins: {summary.TotalRatings}");
        }

        // 2. Multi-role prompt template with systemic analysis instructions
        var promptTemplate = PromptTemplate();

        // 3. Configure low temperature for deterministic, consistent advice
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            MaxTokens = 600
        };

        var arguments = new KernelArguments(executionSettings)
        {
            ["telemetryData"] = summaryText.ToString()
        };


        var streamingResult = _kernel.InvokePromptStreamingAsync(promptTemplate, cancellationToken: ct);

        await foreach (var chunk in streamingResult.WithCancellation(ct))
        {
           var text = chunk.ToString();
           if (!string.IsNullOrEmpty(text))
           {
              yield return text;
           }        
        }
}

    public async Task<string> GenerateCoachingAdviceAsync(List<DomainHarmonySummaryDto> domainSummaries,
     CancellationToken ct = default)
    {
        if (!_isConfigured || _kernel == null)
    {
        // 1. Secondary sort by Category guarantees identical tie-breaking every time
        var lowestDomain = domainSummaries
            .OrderBy(s => s.AverageScore)
            .ThenBy(s => s.Category.ToString())
            .FirstOrDefault();

        var highestDomain = domainSummaries
            .OrderByDescending(s => s.AverageScore)
            .ThenBy(s => s.Category.ToString())
            .FirstOrDefault();

        // 2. Format scores deterministically (+2.38, -0.90, +0.00)
        var highestScoreFormatted = highestDomain?.AverageScore.ToString("+0.00;-0.00;0.00", System.Globalization.CultureInfo.InvariantCulture) ?? "+0.00";
        var lowestScoreFormatted = lowestDomain?.AverageScore.ToString("+0.00;-0.00;0.00", System.Globalization.CultureInfo.InvariantCulture) ?? "-0.00";

        return System.FormattableString.Invariant(
            $"[Mock AI Coaching]: Highest harmony observed in {highestDomain?.Category} ({highestScoreFormatted}). Focus next check-in on {lowestDomain?.Category} ({lowestScoreFormatted}), which shows elevated friction points."
        );
    }
        // 1. Format raw telemetry with contextual descriptions
        var summaryText = new StringBuilder();
        foreach (var summary in domainSummaries)
        {
            summaryText.AppendLine($"- Domain: {summary.Category}");
            summaryText.AppendLine($"  Average Harmony Score: {summary.AverageScore:F2} (Scale: -5.0 High Friction to +5.0 High Harmony)");
            summaryText.AppendLine($"  Logged Check-ins: {summary.TotalRatings}");
        }

        // 2. Multi-role prompt template with systemic analysis instructions
        var promptTemplate = PromptTemplate();
        // 3. Configure low temperature for deterministic, consistent advice
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            MaxTokens = 600
        };

        var arguments = new KernelArguments(executionSettings)
        {
            ["telemetryData"] = summaryText.ToString()
        };

        var result = await _kernel.InvokePromptAsync(promptTemplate, arguments, cancellationToken: ct);
        return result.GetValue<string>() ?? "Unable to generate coaching report at this time.";
    }
}