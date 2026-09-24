using Contracts.Imports;

namespace Application.Imports.Commands;

public sealed record RetryImportJobCommand(int Id);

public sealed class RetryImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse> HandleAsync(RetryImportJobCommand command, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(command, nameof(command), language);
        var job = await ImportJobLookup.GetRequiredAsync(repository, command.Id, language, cancellationToken);
        job.Retry(language);
        await repository.SaveChangesAsync(cancellationToken);
        return job.ToResponse();
    }
}
