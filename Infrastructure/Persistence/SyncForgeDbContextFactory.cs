using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public sealed class SyncForgeDbContextFactory : IDesignTimeDbContextFactory<SyncForgeDbContext>
{
    public SyncForgeDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SYNCFORGE_CONNECTION_STRING")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SyncForge;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<SyncForgeDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new SyncForgeDbContext(options);
    }
}
