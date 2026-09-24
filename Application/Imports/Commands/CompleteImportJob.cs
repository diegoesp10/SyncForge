using Contracts.Imports;

namespace Application.Imports.Commands;

public sealed record CompleteImportJobCommand(int Id);

public sealed class CompleteImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse> HandleAsync(CompleteImportJobCommand command, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(command, nameof(command), language);
        var job = await ImportJobLookup.GetRequiredAsync(repository, command.Id, language, cancellationToken);
        job.Complete(language);
        await repository.SaveChangesAsync(cancellationToken);
        return job.ToResponse(language);
    }
}
