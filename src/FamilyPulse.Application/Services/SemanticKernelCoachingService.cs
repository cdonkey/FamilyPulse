using System.Text;
using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace FamilyPulse.Application.Services;

public class SemanticKernelCoachingService : ICoachingService
{
    private readonly Kernel? _kernel;
    private readonly bool _isConfigured;

    public SemanticKernelCoachingService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(modelId, apiKey);
            _kernel = builder.Build();
            _isConfigured = true;
        }
    }

    public async Task<string> GenerateCoachingAdviceAsync(List<DomainHarmonySummaryDto> domainSummaries, CancellationToken ct = default)
    {
        // Fallback for local development when no OpenAI API key is supplied
        if (!_isConfigured || _kernel == null)
        {
            var lowestDomain = domainSummaries.OrderBy(s => s.AverageScore).FirstOrDefault();
            var highestDomain = domainSummaries.OrderByDescending(s => s.AverageScore).FirstOrDefault();

            return $"[Mock AI Coaching]: Highest harmony observed in {highestDomain?.Category} (+{highestDomain?.AverageScore}). " +
                   $"Focus next check-in on {lowestDomain?.Category} ({lowestDomain?.AverageScore}), which shows elevated friction points.";
        }

        // Format telemetry context into prompt template
        var summaryText = new StringBuilder();
        foreach (var summary in domainSummaries)
        {
            summaryText.AppendLine($"- {summary.Category}: Average Score = {summary.AverageScore} across {summary.TotalRatings} check-ins.");
        }

        var promptTemplate = """
            You are FamilyPulse AI, an empathetic and analytical family harmony coach.
            Analyze the following 12-month family domain telemetry data where scores range from -5 (high friction) to +5 (high harmony):

            {{$telemetryData}}

            Provide a concise 2-sentence coaching insight:
            1. Highlight the strongest domain of alignment.
            2. Provide one actionable recommendation for addressing the lowest-scoring domain.
            """;

        var arguments = new KernelArguments
        {
            ["telemetryData"] = summaryText.ToString()
        };

        var result = await _kernel.InvokePromptAsync(promptTemplate, arguments, cancellationToken: ct);
        return result.GetValue<string>() ?? "Unable to generate coaching report at this time.";
    }
}