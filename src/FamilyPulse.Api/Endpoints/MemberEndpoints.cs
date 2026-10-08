using FamilyPulse.Api.Filters;
using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Application.Members.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FamilyPulse.Api.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/families/{familyId:guid}/members")
                       .WithTags("Family Members");

        // 1. List all members for a family
        group.MapGet("/", async (
            Guid familyId, 
            IMemberService memberService, 
            CancellationToken ct) =>
        {
            var members = await memberService.GetMembersAsync(familyId, ct);
            return Results.Ok(members);
        });

        // 2. Add a new member
        group.MapPost("/", async (
            Guid familyId,
            [FromBody] AddMemberCommand command,
            IMemberService memberService,
            CancellationToken ct) =>
        {
            var member = await memberService.AddMemberAsync(familyId, command, ct);
            return member is not null 
                ? Results.Created($"/api/families/{familyId}/members/{member.Id}", member)
                : Results.NotFound(new { Error = "Family account not found." });
        })
        .AddEndpointFilter<ValidationFilter<AddMemberCommand>>();

        // 3. Update an existing member
        group.MapPut("/{memberId:guid}", async (
            Guid familyId,
            Guid memberId,
            [FromBody] UpdateMemberCommand command,
            IMemberService memberService,
            CancellationToken ct) =>
        {
            var updatedMember = await memberService.UpdateMemberAsync(familyId, memberId, command, ct);
            return updatedMember is not null 
                ? Results.Ok(updatedMember)
                : Results.NotFound(new { Error = "Member or family account not found." });
        })
        .AddEndpointFilter<ValidationFilter<UpdateMemberCommand>>();

        // 4. Remove a member
        group.MapDelete("/{memberId:guid}", async (
            Guid familyId,
            Guid memberId,
            IMemberService memberService,
            CancellationToken ct) =>
        {
            var success = await memberService.DeleteMemberAsync(familyId, memberId, ct);
            return success 
                ? Results.NoContent() 
                : Results.NotFound(new { Error = "Member or family account not found." });
        });
    }
}