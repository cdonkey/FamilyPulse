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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


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
    }
}