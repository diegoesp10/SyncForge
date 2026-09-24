using Domain.Files;

namespace Application.Files;

public interface IFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredFile>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredFile>> ListQueuedAsync(CancellationToken cancellationToken = default);
    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);
    void Remove(StoredFile file);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
