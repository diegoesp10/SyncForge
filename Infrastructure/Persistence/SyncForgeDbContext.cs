using Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class SyncForgeDbContext(DbContextOptions<SyncForgeDbContext> options) : DbContext(options)
{
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SyncForgeDbContext).Assembly);
    }
}
