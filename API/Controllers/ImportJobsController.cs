using API.Errors;
using Application.Imports;
using Contracts.Imports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/import-jobs")]
public sealed class ImportJobsController(IImportJobService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ImportJobResponse>> List(
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.ListAsync(skip, take, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("{id:int}")]
    public Task<ImportJobResponse> GetById(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost]
    public async Task<ActionResult<ImportJobResponse>> Create(
        [FromBody] CreateImportJobRequest request,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var response = await service.CreateAsync(request, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, language }, response);
    }

    [HttpPost("{id:int}/start")]
    public Task<ImportJobResponse> Start(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.StartAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost("{id:int}/complete")]
    public Task<ImportJobResponse> Complete(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.CompleteAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost("{id:int}/fail")]
    public Task<ImportJobResponse> Fail(
        int id, [FromBody] FailImportJobRequest request,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.FailAsync(id, request, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost("{id:int}/retry")]
    public Task<ImportJobResponse> Retry(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.RetryAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
}
