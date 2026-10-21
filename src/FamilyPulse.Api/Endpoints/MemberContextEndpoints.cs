using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Contexts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FamilyPulse.Api.Endpoints;

public static class MemberContextEndpoints
{
    public static void MapMemberContextEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/families/{familyId:guid}/members/{memberId:guid}/context")
                       .WithTags("Member Context Vault");

        // GET: Retrieve context vault for a specific family member
        group.MapGet("/", async (
            Guid familyId,
            Guid memberId,
            IMemberContextService contextService,
            CancellationToken ct) =>
        {
            var context = await contextService.GetContextForMemberAsync(familyId, memberId, ct);
            return context is not null ? Results.Ok(context) : Results.NotFound();
        });

        // PUT: Update explicit user-defined preferences and demographics
        group.MapPut("/preferences", async (
            Guid familyId,
            Guid memberId,
            [FromBody] UpdateMemberPreferencesCommand command,
            IMemberContextService contextService,
            CancellationToken ct) =>
        {
            try
            {
                var updated = await contextService.UpsertPreferencesAsync(familyId, memberId, command, ct);
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { Error = ex.Message });
            }
        });

        // POST: Record an AI-extracted insight (used by background Grok orchestrator)
        group.MapPost("/ai-memories", async (
            Guid familyId,
            Guid memberId,
            [FromBody] RecordAiInsightCommand command,
            IMemberContextService contextService,
            CancellationToken ct) =>
        {
            var success = await contextService.AddAiMemoryAsync(familyId, memberId, command, ct);
            return success 
                ? Results.NoContent() 
                : Results.NotFound(new { Error = "Member or Family Account not found." });
        });
    }
}