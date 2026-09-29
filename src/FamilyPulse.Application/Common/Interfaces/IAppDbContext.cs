using FamilyPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyPulse.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Member> Members { get; }
    DbSet<Rating> Ratings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}