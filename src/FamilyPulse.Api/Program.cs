using System.Text.Json.Serialization;
using FamilyPulse.Application.Dtos;
using FamilyPulse.Application.Services;
using FamilyPulse.Domain.Entities;
using FamilyPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FamilyPulse.Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//JSON Serializer to display Enums as string names
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// 1. Register OpenAPI / Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FamilyPulse API", Version = "v1" });
});

// 2. Register EF Core AppDbContext with SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=familypulse.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Bind IAppDbContext interface to AppDbContext implementation
builder.Services.AddScoped<IAppDbContext>(provider => 
    provider.GetRequiredService<AppDbContext>());


// 3. Register Application Layer Services
builder.Services.AddScoped<HarmonyService>();

builder.Services.AddSingleton<ICoachingService, SemanticKernelCoachingService>();
builder.Services.AddScoped<HarmonyService>();



var app = builder.Build();

// 4. Configure Development Middleware & Database Seeding
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FamilyPulse API v1"));
}

// Automatically create database schema and seed 52 weeks of initial telemetry on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}

// 5. Minimal API Endpoints

// GET /api/members - List all family members
app.MapGet("/api/members", async (AppDbContext db, CancellationToken ct) =>
{
    var members = await db.Members
        .AsNoTracking()
        .Select(m => new MemberDto(m.Id, m.Name, m.Role))
        .ToListAsync(ct);

    return Results.Ok(members);
})
.WithName("GetMembers")
.WithTags("Members");

// POST /api/ratings - Submit a new micro-rating (-5 to +5)
app.MapPost("/api/ratings", async (CreateRatingDto dto, AppDbContext db, CancellationToken ct) =>
{
    try
    {
        var rating = new Rating(
            dto.EvaluatorId,
            dto.RecipientId,
            dto.Category,
            dto.Score,
            dto.Note
        );

        db.Ratings.Add(rating);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/ratings/{rating.Id}", rating.Id);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateRating")
.WithTags("Ratings");

// GET /api/reports/annual-harmony
app.MapGet("/api/reports/annual-harmony", async (HarmonyService harmonyService, CancellationToken ct) =>
{
    var report = await harmonyService.GetAnnualHarmonyReportAsync(ct);
    return Results.Ok(report);
})
.WithName("GetAnnualHarmonyReport")
.WithTags("Reports");


app.MapGet("/", () => Results.Redirect("/swagger"));



app.Run();