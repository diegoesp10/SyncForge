using API.Errors;
using Application.Orders;
using Contracts.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OrderResponse>> List(
        [FromQuery] int importJobId, [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.ListAsync(importJobId, skip, take, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("{id:int}")]
    public Task<OrderResponse> GetById(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("by-source")]
    public Task<OrderResponse> GetBySource(
        [FromQuery] string sourceSystem, [FromQuery] string externalId,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetBySourceAsync(sourceSystem, externalId, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var response = await service.CreateAsync(request, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, language }, response);
    }
}
