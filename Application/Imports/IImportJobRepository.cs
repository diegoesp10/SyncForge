using Domain.Imports;

namespace Application.Imports;

public interface IImportJobRepository
{
    Task<ImportJob?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImportJob>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync(ImportJob job, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
