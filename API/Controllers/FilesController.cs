using API.Errors;
using Application.Files;
using Contracts.Files;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FilesController(IFileService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<FileItemResponse>> List(
        [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.ListAsync(ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<FileItemResponse> GetById(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpGet("{id:guid}/result")]
    public Task<FileResultResponse> GetResult(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.GetResultAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FileService.MaxFileSizeBytes + 1024 * 1024)]
    public async Task<ActionResult<FileItemResponse>> Upload(
        [FromForm] IFormFile file,
        [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        await using var content = file.OpenReadStream();
        var response = await service.UploadAsync(file.FileName, file.ContentType, file.Length,
            content, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id, language }, response);
    }

    [HttpPost("{id:guid}/reprocess")]
    public Task<FileItemResponse> Reprocess(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default) =>
        service.ReprocessAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id, [FromQuery] string? language = null, CancellationToken cancellationToken = default)
    {
        await service.DeleteAsync(id, ApiLanguage.Resolve(HttpContext, language), cancellationToken);
        return NoContent();
    }
}
