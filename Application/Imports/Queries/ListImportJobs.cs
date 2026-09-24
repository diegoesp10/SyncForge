using Contracts.Imports;
using Domain.Resources;

namespace Application.Imports.Queries;

public sealed record ListImportJobsQuery(int Skip = 0, int Take = 50);

public sealed class ListImportJobsHandler(IImportJobRepository repository)
{
    public async Task<IReadOnlyList<ImportJobResponse>> HandleAsync(ListImportJobsQuery query, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(query, nameof(query), language);
        if (query.Skip < 0 || query.Take is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(query), ErrorMessages.Get(ErrorCode.InvalidPagination, language));

        var jobs = await repository.ListAsync(query.Skip, query.Take, cancellationToken);
        return jobs.Select(job => job.ToResponse(language)).ToArray();
    }
}
