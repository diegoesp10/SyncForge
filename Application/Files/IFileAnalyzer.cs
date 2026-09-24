using Contracts.Files;

namespace Application.Files;

public interface IFileAnalyzer
{
    Task<FileResultResponse> AnalyzeAsync(Guid id, string fileName, Stream content, CancellationToken cancellationToken = default);
}
