using Contracts.Imports;

namespace Application.Imports;

public interface IImportJobService
{
    Task<IReadOnlyList<ImportJobResponse>> ListAsync(int skip, int take, string language, CancellationToken cancellationToken = default);
    Task<ImportJobResponse> GetAsync(int id, string language, CancellationToken cancellationToken = default);
    Task<ImportJobResponse> CreateAsync(CreateImportJobRequest request, string language, CancellationToken cancellationToken = default);
    Task<ImportJobResponse> StartAsync(int id, string language, CancellationToken cancellationToken = default);
    Task<ImportJobResponse> CompleteAsync(int id, string language, CancellationToken cancellationToken = default);
    Task<ImportJobResponse> FailAsync(int id, FailImportJobRequest request, string language, CancellationToken cancellationToken = default);
    Task<ImportJobResponse> RetryAsync(int id, string language, CancellationToken cancellationToken = default);
}
