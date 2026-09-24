using Contracts.Orders;
using Domain.Resources;

namespace Application.Orders.Queries;

public sealed record ListOrdersQuery(int ImportJobId, int Skip = 0, int Take = 50);

public sealed class ListOrdersHandler(IOrderRepository repository, Application.Imports.IImportJobRepository importJobs)
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(ListOrdersQuery query, string language, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ImportJobId <= 0)
            throw new ArgumentOutOfRangeException(nameof(query), ErrorMessages.Get(ErrorCode.InvalidImportJobId, language));
        if (query.Skip < 0 || query.Take is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(query), ErrorMessages.Get(ErrorCode.InvalidPagination, language));
        if (await importJobs.GetByIdAsync(query.ImportJobId, cancellationToken) is null)
            throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.ImportJobNotFound, language, query.ImportJobId));

        var orders = await repository.ListByImportJobAsync(query.ImportJobId, query.Skip, query.Take, cancellationToken);
        return orders.Select(order => order.ToResponse()).ToArray();
    }
}
