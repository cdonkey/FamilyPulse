using System.Net;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Domain.Enums;
using FamilyPulse.Infrastructure.Data;
using FamilyPulse.Testing.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FamilyPulse.Integration.Tests;

public class CoachingEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CoachingEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCoachingStream_Returns200OK_AndStreamsServerSentEvents()
    {
        // Arrange: Seed database via Factory's Service Provider
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var familyId = Guid.NewGuid();
            var memberId = Guid.NewGuid();

            db.CheckIns.Add(new CheckIn(
                familyId, 
                memberId, 
                CheckInType.WeeklyRoutine, 
                DomainCategory.Marriage, 
                stressLevel: 4, 
                closenessLevel: 3,
                energyLevel:3,
                note: "Feeling overloaded this week"
            ));
            await db.SaveChangesAsync();

            // Act: Call the HTTP API streaming endpoint
            var client = _factory.CreateClient();
            var response = await client.GetAsync(
                $"/api/members/{memberId}/coaching/stream?familyAccountId={familyId}", 
                HttpCompletionOption.ResponseHeadersRead);


            if (response.StatusCode != HttpStatusCode.OK)
{
           var errorBody = await response.Content.ReadAsStringAsync();
     throw new Exception($"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorBody}");
}    

            // Assert: Verify HTTP pipeline, headers, and SSE content
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

            var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var firstChunk = await reader.ReadLineAsync();

            Assert.NotNull(firstChunk);
            Assert.StartsWith("data:", firstChunk);
        }
    }
}