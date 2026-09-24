using Application.Files;
using Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class TrashCanRepository(SyncForgeDbContext dbContext) : ITrashCanRepository
{
    public Task<TrashCanEntry?> GetByFileIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.TrashCan.Include(entry => entry.File)
            .SingleOrDefaultAsync(entry => entry.FileId == id, cancellationToken);

    public async Task<IReadOnlyList<TrashCanEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.TrashCan.AsNoTracking().Include(entry => entry.File)
            .OrderByDescending(entry => entry.MovedAt)
            .ThenByDescending(entry => entry.FileId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListExpiredIdsAsync(DateTimeOffset now, CancellationToken cancellationToken = default) =>
        await dbContext.TrashCan.AsNoTracking()
            .Where(entry => entry.PurgeAt <= now)
            .OrderBy(entry => entry.PurgeAt)
            .Select(entry => entry.FileId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TrashCanEntry entry, CancellationToken cancellationToken = default) =>
        await dbContext.TrashCan.AddAsync(entry, cancellationToken);

    public void Remove(TrashCanEntry entry) => dbContext.TrashCan.Remove(entry);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
