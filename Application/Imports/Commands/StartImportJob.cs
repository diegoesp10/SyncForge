using Contracts.Imports;

namespace Application.Imports.Commands;

public sealed record StartImportJobCommand(int Id);

public sealed class StartImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse> HandleAsync(StartImportJobCommand command, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(command, nameof(command), language);
        var job = await ImportJobLookup.GetRequiredAsync(repository, command.Id, language, cancellationToken);
        job.Start(language);
        await repository.SaveChangesAsync(cancellationToken);
        return job.ToResponse(language);
    }
}
