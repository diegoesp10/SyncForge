using Application.Health;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class SqlHealthProbe(SyncForgeDbContext dbContext) : IHealthProbe
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) =>
        dbContext.Database.CanConnectAsync(cancellationToken);
}
