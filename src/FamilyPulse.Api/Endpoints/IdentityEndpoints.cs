using FamilyPulse.Api.Filters;
using FamilyPulse.Application.Identity.Commands;
using FamilyPulse.Application.Common.Interfaces;

namespace FamilyPulse.Api.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity")
                       .WithTags("Anonymous Identity");

        group.MapPost("/register", async (
            RegisterAnonymousFamilyCommand command,
            IIdentityService identityService,
            CancellationToken ct) =>
        {
            var response = await identityService.RegisterAsync(command, ct);
            return Results.Created($"/api/identity/{response.FamilyId}", response);
        })
        .AddEndpointFilter<ValidationFilter<RegisterAnonymousFamilyCommand>>(); // Filter runs first!
    }
}