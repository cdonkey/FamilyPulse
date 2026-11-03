using FamilyPulse.Application.Coaching;

namespace FamilyPulse.Api.Endpoints;

public static class CoachingEndpoints
{
 public static void MapCoachingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/members/{memberId:guid}/coaching/stream", async (
            Guid memberId,
            Guid familyAccountId,
            IAiCoachingOrchestrator orchestrator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            httpContext.Response.ContentType = "text/event-stream";

            // Correct: Use 'await foreach' for IAsyncEnumerable
            await foreach (var chunk in orchestrator.GenerateRecommendationAsync(familyAccountId, memberId, ct))
            {
                await httpContext.Response.WriteAsync($"data: {chunk}\n\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }
        });
    }    
}