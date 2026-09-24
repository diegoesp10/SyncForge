using Domain.Files;

namespace Application.Files;

public interface ITrashCanRepository
{
    Task<TrashCanEntry?> GetByFileIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrashCanEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> ListExpiredIdsAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task AddAsync(TrashCanEntry entry, CancellationToken cancellationToken = default);
    void Remove(TrashCanEntry entry);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
