using FamilyPulse.Domain.Entities;
using FamilyPulse.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using  System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Testing.Shared;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Remove the production DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // 2. Keep a single in-memory connection open for the lifetime of the factory
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // 3. Register DbContext using the in-memory connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

          services.AddControllers()
            .AddJsonOptions(options =>
          {
                 options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
            
            // Program.cs will now execute MigrateAsync() and SeedAsync() automatically when the test server starts!
        });
    }

    // Helper method to seed initial test state
    public async Task<(Guid FamilyId, Guid MemberId)> SeedFamilyAndMemberAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var family = new FamilyAccount($"test-family-{Guid.NewGuid():N}");




        var member = new Member(family.Id, "Alex", "Parent");

        db.FamilyAccounts.Add(family);
        db.Members.Add(member);
        await db.SaveChangesAsync();

        return (family.Id, member.Id);
    }


    

    public void InitializeDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}