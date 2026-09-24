using Contracts.Imports;

namespace Application.Imports.Commands;

public sealed record FailImportJobCommand(int Id, string ErrorMessage);

public sealed class FailImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse> HandleAsync(FailImportJobCommand command, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(command, nameof(command), language);
        var job = await ImportJobLookup.GetRequiredAsync(repository, command.Id, language, cancellationToken);
        job.Fail(command.ErrorMessage, language);
        await repository.SaveChangesAsync(cancellationToken);
        return job.ToResponse();
    }
}
