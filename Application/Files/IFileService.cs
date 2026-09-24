using Contracts.Files;

namespace Application.Files;

public interface IFileService
{
    Task<FileItemResponse> UploadAsync(string fileName, string? contentType, long size, Stream content, string language, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileItemResponse>> ListAsync(string language, CancellationToken cancellationToken = default);
    Task<FileItemResponse> GetAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task<FileResultResponse> GetResultAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task<FileItemResponse> ReprocessAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string language, CancellationToken cancellationToken = default);
}
