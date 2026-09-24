using Contracts.Orders;
using Domain.Resources;

namespace Application.Orders.Queries;

public sealed record GetOrderBySourceQuery(string SourceSystem, string ExternalId);

public sealed class GetOrderBySourceHandler(IOrderRepository repository)
{
    public async Task<OrderResponse?> HandleAsync(GetOrderBySourceQuery query, string language, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.SourceSystem) || string.IsNullOrWhiteSpace(query.ExternalId))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidRequest, language), nameof(query));

        var order = await repository.GetBySourceAsync(query.SourceSystem.Trim(), query.ExternalId.Trim(), cancellationToken);
        return order?.ToResponse();
    }
}
