using FamilyPulse.Domain.Entities;
using FamilyPulse.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace FamilyPulse.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing AppDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Open a persistent SQLite in-memory connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Ensure schema is created for SQLite in-memory
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    // Helper method to seed initial test state
    public async Task<(Guid FamilyId, Guid MemberId)> SeedFamilyAndMemberAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var family = new FamilyAccount("Test Family");
        var member = new Member(family.Id, "Alex", "Parent");

        db.FamilyAccounts.Add(family);
        db.Members.Add(member);
        await db.SaveChangesAsync();

        return (family.Id, member.Id);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}