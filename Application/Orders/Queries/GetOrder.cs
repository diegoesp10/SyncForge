using Contracts.Orders;

namespace Application.Orders.Queries;

public sealed record GetOrderQuery(int Id);

public sealed class GetOrderHandler(IOrderRepository repository)
{
    public async Task<OrderResponse?> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var order = await repository.GetByIdAsync(query.Id, cancellationToken);
        return order?.ToResponse();
    }
}
