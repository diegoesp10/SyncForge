using API.Errors;
using Application.Imports.Commands;
using Application.Imports.Queries;
using Contracts.Imports;
using Domain.Imports.Enums;
using Domain.Resources;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/import-jobs")]
public sealed class ImportJobsController(
    CreateImportJobHandler create,
    GetImportJobHandler get,
    ListImportJobsHandler list,
    StartImportJobHandler start,
    CompleteImportJobHandler complete,
    FailImportJobHandler fail,
    RetryImportJobHandler retry) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ImportJobResponse>>> List(
        [FromQuery] int skip = 0, [FromQuery] int take = 50,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        Ok(await list.HandleAsync(new ListImportJobsQuery(skip, take), ApiLanguage.Resolve(HttpContext, language), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ImportJobResponse>> GetById(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var selectedLanguage = ApiLanguage.Resolve(HttpContext, language);
        return await get.HandleAsync(new GetImportJobQuery(id), selectedLanguage, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.ImportJobNotFound, selectedLanguage, id));
    }

    [HttpPost]
    public async Task<ActionResult<ImportJobResponse>> Create(
        [FromBody] CreateImportJobRequest request,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var selectedLanguage = ApiLanguage.Resolve(HttpContext, language);
        if (!Enum.TryParse<ImportFormat>(request.Format, true, out var format) || !Enum.IsDefined(format))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidFormat, selectedLanguage), nameof(request));

        var response = await create.HandleAsync(new CreateImportJobCommand(
            request.SourceSystem, request.FileName, request.StoredFileKey, format), selectedLanguage, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, language }, response);
    }

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<ImportJobResponse>> Start(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        Ok(await start.HandleAsync(new StartImportJobCommand(id), ApiLanguage.Resolve(HttpContext, language), cancellationToken));

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<ImportJobResponse>> Complete(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        Ok(await complete.HandleAsync(new CompleteImportJobCommand(id), ApiLanguage.Resolve(HttpContext, language), cancellationToken));

    [HttpPost("{id:int}/fail")]
    public async Task<ActionResult<ImportJobResponse>> Fail(
        int id, [FromBody] FailImportJobRequest request,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        var selectedLanguage = ApiLanguage.Resolve(HttpContext, language);
        if (!Enum.TryParse<ImportFailureCode>(request.FailureCode, true, out var failureCode) || !Enum.IsDefined(failureCode))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidFailureCode, selectedLanguage), nameof(request));

        return Ok(await fail.HandleAsync(new FailImportJobCommand(id, failureCode), selectedLanguage, cancellationToken));
    }

    [HttpPost("{id:int}/retry")]
    public async Task<ActionResult<ImportJobResponse>> Retry(
        int id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        Ok(await retry.HandleAsync(new RetryImportJobCommand(id), ApiLanguage.Resolve(HttpContext, language), cancellationToken));
}
