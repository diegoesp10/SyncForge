using Contracts.Imports;

namespace Application.Imports.Queries;

public sealed record GetImportJobQuery(int Id);

public sealed class GetImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse?> HandleAsync(GetImportJobQuery query, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(query, nameof(query), language);
        var job = await repository.GetByIdAsync(query.Id, cancellationToken);
        return job?.ToResponse(language);
    }
}
