using API.Errors;
using Application.Files;
using Contracts.Files;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/trash-can")]
public sealed class TrashCanController(ITrashCanService service) : ControllerBase
{
    [HttpPost("{id:guid}")]
    public async Task<ActionResult<TrashCanFileResponse>> MoveToTrashCan(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var response = await service.MoveToTrashCanAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id, language }, response);
    }

    [HttpGet]
    public Task<IReadOnlyList<TrashCanFileResponse>> List(
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.ListAsync(ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<TrashCanFileResponse> GetById(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("{id:guid}/result")]
    public Task<FileResultResponse> GetResult(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetResultAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost("{id:guid}/restore")]
    public Task<FileItemResponse> Restore(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.RestoreAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Purge(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        await service.PurgeAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return NoContent();
    }
}
