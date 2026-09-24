using Contracts.Orders;
using Domain.Orders;

namespace Application.Orders;

internal static class OrderMapping
{
    public static OrderResponse ToResponse(this Order order) => new(
        order.Id,
        order.ImportJobId,
        order.SourceSystem,
        order.ExternalId,
        order.CustomerName,
        order.Amount,
        order.Currency,
        order.CreatedAt);
}
