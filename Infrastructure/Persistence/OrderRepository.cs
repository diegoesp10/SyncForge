using Application.Orders;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class OrderRepository(SyncForgeDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Orders.AsNoTracking()
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> GetBySourceAsync(string sourceSystem, string externalId, CancellationToken cancellationToken = default) =>
        dbContext.Orders.AsNoTracking()
            .SingleOrDefaultAsync(order => order.SourceSystem == sourceSystem && order.ExternalId == externalId, cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByImportJobAsync(int importJobId, int skip, int take, CancellationToken cancellationToken = default) =>
        await dbContext.Orders.AsNoTracking()
            .Where(order => order.ImportJobId == importJobId)
            .OrderBy(order => order.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await dbContext.Orders.AddAsync(order, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
