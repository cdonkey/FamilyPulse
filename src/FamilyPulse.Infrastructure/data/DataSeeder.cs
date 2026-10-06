using FamilyPulse.Domain.Entities;
using FamilyPulse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {

       if (!await db.FamilyAccounts.AnyAsync())
       {
            // Seed Hash corresponding to passphrase + landmark demo seed
            var demoAccount = new FamilyAccount("DEMO_SEED_IDENTITY_HASH_123456789");
            await db.FamilyAccounts.AddAsync(demoAccount);
            await db.SaveChangesAsync();


            if (await db.Members.AnyAsync()) return; // Database already seeded

             var dad = new Member(demoAccount.Id, "Alex", "Parent");
             var mom = new Member(demoAccount.Id, "Sarah", "Parent");
             var child = new Member(demoAccount.Id, "Leo", "Child");

             await db.Members.AddRangeAsync(dad, mom, child);


               var ratings = new List<Rating>();
        var random = new Random(42); // Fixed seed for reproducible benchmarks
        var now = DateTime.UtcNow;

        // Generate 52 weeks of ratings across 4 categories
        for (int week = 52; week >= 0; week--)
        {
            var date = now.AddDays(-7 * week);
            
            foreach (DomainCategory category in Enum.GetValues<DomainCategory>())
            {
                // Create slight variations per domain
                int baseScore = category switch
                {
                    DomainCategory.Financial => random.Next(-3, 3),
                    DomainCategory.Marriage => random.Next(1, 5),
                    DomainCategory.Education => random.Next(0, 4),
                    DomainCategory.Activities => random.Next(-2, 4),
                    _ => 0
                };

                ratings.Add(new Rating(
                    dad.Id, 
                    mom.Id, 
                    category, 
                    baseScore, 
                    $"Weekly check-in for {category}", 
                    date
                ));
            }
        }

        await db.Ratings.AddRangeAsync(ratings);
        await db.SaveChangesAsync();






        }  

        
       
      
    }
}