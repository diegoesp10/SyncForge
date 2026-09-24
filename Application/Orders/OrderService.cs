using Application.Orders.Commands;
using Application.Orders.Queries;
using Contracts.Orders;
using Domain.Resources;

namespace Application.Orders;

public sealed class OrderService(
    CreateOrderHandler create,
    GetOrderHandler get,
    GetOrderBySourceHandler getBySource,
    ListOrdersHandler list) : IOrderService
{
    public Task<IReadOnlyList<OrderResponse>> ListAsync(int importJobId, int skip, int take, string language, CancellationToken cancellationToken = default) =>
        list.HandleAsync(new ListOrdersQuery(importJobId, skip, take), language, cancellationToken);

    public async Task<OrderResponse> GetAsync(int id, string language, CancellationToken cancellationToken = default) =>
        await get.HandleAsync(new GetOrderQuery(id), cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.OrderNotFound, language, id));

    public async Task<OrderResponse> GetBySourceAsync(string sourceSystem, string externalId, string language, CancellationToken cancellationToken = default) =>
        await getBySource.HandleAsync(new GetOrderBySourceQuery(sourceSystem, externalId), language, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.OrderNotFound, language, externalId));

    public Task<OrderResponse> CreateAsync(CreateOrderRequest request, string language, CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request), ErrorMessages.Get(ErrorCode.RequiredValue, language, nameof(request)));

        return create.HandleAsync(new CreateOrderCommand(
            request.ImportJobId, request.SourceSystem, request.ExternalId,
            request.CustomerName, request.Amount, request.Currency), language, cancellationToken);
    }
}
