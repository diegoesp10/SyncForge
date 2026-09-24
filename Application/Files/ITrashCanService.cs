using Contracts.Files;

namespace Application.Files;

public interface ITrashCanService
{
    Task<TrashCanFileResponse> MoveToTrashCanAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrashCanFileResponse>> ListAsync(string language, CancellationToken cancellationToken = default);
    Task<TrashCanFileResponse> GetAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task<FileResultResponse> GetResultAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task<FileItemResponse> RestoreAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task PurgeAsync(Guid id, string language, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> ListExpiredIdsAsync(CancellationToken cancellationToken = default);
}
