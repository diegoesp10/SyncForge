using Domain.Imports;
using Domain.Files;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class SyncForgeDbContext(DbContextOptions<SyncForgeDbContext> options) : DbContext(options)
{
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportAttempt> ImportAttempts => Set<ImportAttempt>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SyncForgeDbContext).Assembly);
    }
}
