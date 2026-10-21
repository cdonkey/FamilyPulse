using FamilyPulse.Api.Filters;
using FamilyPulse.Application.CheckIns.Dtos;
using FamilyPulse.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamilyPulse.Api.Endpoints;

public static class CheckInEndpoints
{
    public static void MapCheckInEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/families/{familyId:guid}/checkins")
                       .WithTags("Family Check-Ins");

        // GET: Recent check-ins for trend analysis
        group.MapGet("/", async (
            Guid familyId,
            ICheckInService checkInService,
            CancellationToken ct,
            [FromQuery] int limit = 20) =>
        {
            var checkIns = await checkInService.GetCheckInsForFamilyAsync(familyId, limit, ct);
            return Results.Ok(checkIns);
        });

        // POST: Record a new check-in (Weekly Routine or Life Event)
        group.MapPost("/", async (
            Guid familyId,
            [FromBody] CreateCheckInCommand command,
            ICheckInService checkInService,
            CancellationToken ct) =>
        {
            var result = await checkInService.CreateCheckInAsync(familyId, command, ct);
            return result is not null
                ? Results.Created($"/api/families/{familyId}/checkins/{result.Id}", result)
                : Results.NotFound(new { Error = "Member or Family Account not found." });
        })
        .AddEndpointFilter<ValidationFilter<CreateCheckInCommand>>();
    }
}