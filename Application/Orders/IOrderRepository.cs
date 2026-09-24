using Domain.Orders;

namespace Application.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetBySourceAsync(string sourceSystem, string externalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByImportJobAsync(int importJobId, int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
