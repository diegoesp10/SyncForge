using Application.Files;
using Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class FileRepository(SyncForgeDbContext dbContext) : IFileRepository
{
    public Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.StoredFiles.SingleOrDefaultAsync(file => file.Id == id, cancellationToken);

    public async Task<IReadOnlyList<StoredFile>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.StoredFiles.AsNoTracking()
            .OrderByDescending(file => file.UploadedAt)
            .ThenByDescending(file => file.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StoredFile>> ListQueuedAsync(CancellationToken cancellationToken = default) =>
        await dbContext.StoredFiles
            .Where(file => file.Status == FileStatus.Pending || file.Status == FileStatus.Processing)
            .OrderBy(file => file.UploadedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(StoredFile file, CancellationToken cancellationToken = default) =>
        await dbContext.StoredFiles.AddAsync(file, cancellationToken);

    public void Remove(StoredFile file) => dbContext.StoredFiles.Remove(file);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
