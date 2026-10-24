using System.Net;
using System.Net.Http.Json;
using FamilyPulse.Application.Contexts.DTOs;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Infrastructure.Data;
using FamilyPulse.Testing.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FamilyPulse.IntegrationTests.Endpoints;


public class MemberContextEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MemberContextEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PutPreferences_CreatesOrUpdatesMemberContext()
    {
        // Arrange
        var (familyId, memberId) = await _factory.SeedFamilyAndMemberAsync();
        var command = new UpdateMemberPreferencesCommand(
            Age: 14,
            CommunicationPreference: "Direct",
            Interests: new List<string> { "Chess", "Coding" },
            TopicsToAvoid: new List<string> { "Bedtime negotiations" }
        );

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/families/{familyId}/members/{memberId}/context/preferences", 
            command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var context = await response.Content.ReadFromJsonAsync<MemberContextDto>();
        Assert.NotNull(context);
        Assert.Equal(14, context.Age);
        Assert.Equal("Direct", context.CommunicationPreference);
        Assert.Contains("Chess", context.Interests);
    }

/*

    [Fact]
    public async Task PostAiMemory_AddsLearnedMemoryToContextVault()
    {
        // Arrange
        var (familyId, memberId) = await _factory.SeedFamilyAndMemberAsync();
        var aiCommand = new RecordAiInsightCommand(
            Category: "Stress Trigger",
            Fact: "Gets anxious before exams",
            Confidence: 0.95
        );

        // Act
        var postResponse = await _client.PostAsJsonAsync(
            $"/api/families/{familyId}/members/{memberId}/context/ai-memories", 
            aiCommand);

        // Assert POST
        Assert.Equal(HttpStatusCode.NoContent, postResponse.StatusCode);

        // Verify state via GET
        var getResponse = await _client.GetAsync(
            $"/api/families/{familyId}/members/{memberId}/context");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var context = await getResponse.Content.ReadFromJsonAsync<MemberContextDto>();
        Assert.NotNull(context);
        var memory = Assert.Single(context.LearnedMemories);
        Assert.Equal("Stress Trigger", memory.Category);
        Assert.Equal("Gets anxious before exams", memory.Fact);
    }



*/



[Fact]
public async Task PostAiMemory_AddsLearnedMemoryToContextVault()
{
    // 1. Arrange: Ensure parent account/member exist in DB
    using (var setupScope = _factory.Services.CreateScope())
    {
        var setupDb = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var account = await setupDb.FamilyAccounts.FirstOrDefaultAsync() 
                      ?? new FamilyAccount($"test-{Guid.NewGuid():N}");
        if (setupDb.Entry(account).State == EntityState.Detached) 
            setupDb.FamilyAccounts.Add(account);

        var member = await setupDb.Members.FirstOrDefaultAsync(m => m.FamilyAccountId == account.Id);

        if (member == null)
        {
            member = new Member(account.Id, "Maria", "Parent");
            
            setupDb.Members.Add(member);
        }

        await setupDb.SaveChangesAsync();
    }

    // Retrieve target IDs
    Guid testMemberId;
    using (var idScope = _factory.Services.CreateScope())
    {
        var db = idScope.ServiceProvider.GetRequiredService<AppDbContext>();
        testMemberId = (await db.Members.FirstAsync()).Id;
    }

    var requestPayload = new
    {
        Content = "Prefers morning workout sessions",
        Category = "Routine",
        Source = "GrokAI"
    };

    // 2. Act: Send POST request
    var response = await _client.PostAsJsonAsync($"/api/members/{testMemberId}/memories", requestPayload);
    
    // Ensure HTTP request succeeded (triggers clear error if endpoint fails)
    response.EnsureSuccessStatusCode();

    // 3. Assert: Query DB using a FRESH scope created AFTER the HTTP call
    using (var verifyScope = _factory.Services.CreateScope())
    {
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        


                
        
        // Query database directly for saved memory entries
        var memories = await verifyDb.Set<MemberContextMemory>()
            .Where(m => m.MemberId == testMemberId)
            .ToListAsync();

        Assert.Single(memories);
        Assert.Equal("Prefers morning workout sessions", memories.First().Content);
    }
  }

}