using System.Text.Json;
using FamilyPulse.Application.Common.Interfaces;
using FamilyPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Infrastructure.Data;


public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<FamilyAccount> FamilyAccounts => Set<FamilyAccount>();

    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    public DbSet<MemberContextMemory> MemberContextMemories => Set<MemberContextMemory>();


    public DbSet<MemberContext> MemberContexts => Set<MemberContext>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<AiMemoryItem>();

        modelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());



        

    modelBuilder.Entity<FamilyAccount>(b =>
    {

       b.HasKey(a => a.Id);
    
       b.Property(a => a.IdentityHash)
        .IsRequired()
        .HasMaxLength(256);

      b.HasIndex(a => a.IdentityHash)
        .IsUnique();

       // 1-to-Many Relationship: FamilyAccount -> Members
      b.HasMany(a => a.Members)
        .WithOne()
        .HasForeignKey(m => m.FamilyAccountId)
        .OnDelete(DeleteBehavior.Cascade);
      });


        modelBuilder.Entity<Member>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Name).IsRequired().HasMaxLength(100);
            b.Property(m => m.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Rating>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Score).IsRequired();
            b.Property(r => r.Category).HasConversion<int>().IsRequired();
            b.Property(r => r.Note).HasMaxLength(500);
            b.HasIndex(r => r.CreatedAtUtc);
        });



        modelBuilder.Entity<CheckIn>(b =>
        {
           b.HasKey(c => c.Id);
           b.Property(c => c.Tags)
           .HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()); 
        });

        modelBuilder.Entity<MemberContextMemory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.HasOne(e => e.Member)
                  .WithMany()
                  .HasForeignKey(e => e.MemberId)
                  .OnDelete(DeleteBehavior.Cascade);
        });




    }

    
}