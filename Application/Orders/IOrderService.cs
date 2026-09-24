using Contracts.Orders;

namespace Application.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponse>> ListAsync(int importJobId, int skip, int take, string language, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetAsync(int id, string language, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetBySourceAsync(string sourceSystem, string externalId, string language, CancellationToken cancellationToken = default);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, string language, CancellationToken cancellationToken = default);
}
