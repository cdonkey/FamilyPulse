using FamilyPulse.Api.Filters;
using FamilyPulse.Application.Identity.Commands;
using FamilyPulse.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

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


   group.MapPost("/recover", async (
    [FromBody] Application.Identity.Queries.RecoverAccountQuery query,
    IIdentityService identityService,
    CancellationToken ct) =>
{
    var response = await identityService.RecoverAsync(query, ct);

    // If account was found & verified, return 200 OK. 
    // If not found / invalid key, return 404 Not Found with an error payload.
    return response is not null 
        ? Results.Ok(response) 
        : Results.NotFound(new { Error = "Account not found or invalid recovery credentials." });
});
    







    }


     
}