using Application.Imports;
using Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ImportJobRepository(SyncForgeDbContext dbContext) : IImportJobRepository
{
    public Task<ImportJob?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.ImportJobs.Include(job => job.Attempts)
            .SingleOrDefaultAsync(job => job.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ImportJob>> ListAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        await dbContext.ImportJobs
            .AsNoTracking()
            .AsSplitQuery()
            .Include(job => job.Attempts)
            .OrderByDescending(job => job.CreatedAt)
            .ThenByDescending(job => job.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ImportJob job, CancellationToken cancellationToken = default) =>
        await dbContext.ImportJobs.AddAsync(job, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
