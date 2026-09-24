using API.Errors;
using Application.Orders.Commands;
using Application.Orders.Queries;
using Contracts.Orders;
using Domain.Resources;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(
    CreateOrderHandler create,
    GetOrderHandler get,
    GetOrderBySourceHandler getBySource,
    ListOrdersHandler list) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> List(
        [FromQuery] int importJobId, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        Ok(await list.HandleAsync(new ListOrdersQuery(importJobId, skip, take), ApiLanguage.Resolve(HttpContext, language), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var selectedLanguage = ApiLanguage.Resolve(HttpContext, language);
        return await get.HandleAsync(new GetOrderQuery(id), cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.OrderNotFound, selectedLanguage, id));
    }

    [HttpGet("by-source")]
    public async Task<ActionResult<OrderResponse>> GetBySource(
        [FromQuery] string sourceSystem, [FromQuery] string externalId,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var selectedLanguage = ApiLanguage.Resolve(HttpContext, language);
        return await getBySource.HandleAsync(new GetOrderBySourceQuery(sourceSystem, externalId), selectedLanguage, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.OrderNotFound, selectedLanguage, externalId));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var response = await create.HandleAsync(new CreateOrderCommand(
            request.ImportJobId, request.SourceSystem, request.ExternalId,
            request.CustomerName, request.Amount, request.Currency), ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, language }, response);
    }
}
