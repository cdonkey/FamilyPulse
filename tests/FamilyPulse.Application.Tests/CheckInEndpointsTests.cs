using Xunit; // Provides IClassFixture<T>, [Fact], and Assert
using Microsoft.AspNetCore.Mvc.Testing; // Provides WebApplicationFactory<T>
using System.Net;
using System.Net.Http.Json;
using FamilyPulse.Application.CheckIns.Dtos;
using FamilyPulse.IntegrationTests.Infrastructure;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Application.Tests;
namespace FamilyPulse.IntegrationTests.Endpoints;


public class CheckInEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CheckInEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCheckIn_WithValidData_Returns201CreatedAndDto()
    {
        // Arrange
        var (familyId, memberId) = await _factory.SeedFamilyAndMemberAsync();
        var command = new CreateCheckInCommand(
            MemberId: memberId,
            Type: CheckInType.WeeklyRoutine,
            StressLevel: 2,
            ClosenessLevel: 5,
            EnergyLevel: 4,
            Note: "Great weekend family hike",
            Tags: new List<string> { "Outdoor", "Exercise" }
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/families/{familyId}/checkins", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CheckInDto>();
        Assert.NotNull(result);
        Assert.Equal(familyId, result.FamilyAccountId);
        Assert.Equal(memberId, result.MemberId);
        Assert.Equal(2, result.StressLevel);
        Assert.Contains("Outdoor", result.Tags);
    }

    [Fact]
    public async Task GetCheckIns_ReturnsRecentCheckInsList()
    {
        // Arrange
        var (familyId, memberId) = await _factory.SeedFamilyAndMemberAsync();
        var command = new CreateCheckInCommand(memberId, CheckInType.LifeEvent, 4, 3, 2, "Event note", null);
        await _client.PostAsJsonAsync($"/api/families/{familyId}/checkins", command);

        // Act
        var response = await _client.GetAsync($"/api/families/{familyId}/checkins?limit=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var checkIns = await response.Content.ReadFromJsonAsync<List<CheckInDto>>();
        Assert.NotNull(checkIns);
        Assert.NotEmpty(checkIns);
    }
}