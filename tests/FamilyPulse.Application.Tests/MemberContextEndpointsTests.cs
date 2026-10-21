using System.Net;
using System.Net.Http.Json;
using FamilyPulse.Application.Contexts.DTOs;

using FamilyPulse.IntegrationTests;
using FamilyPulse.IntegrationTests.Infrastructure;
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
}