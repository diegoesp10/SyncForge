using Contracts.Imports;
using Domain.Imports.Enums;

namespace Application.Imports.Commands;

public sealed record FailImportJobCommand(int Id, ImportFailureCode FailureCode);

public sealed class FailImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse> HandleAsync(FailImportJobCommand command, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(command, nameof(command), language);
        var job = await ImportJobLookup.GetRequiredAsync(repository, command.Id, language, cancellationToken);
        job.Fail(command.FailureCode, language);
        await repository.SaveChangesAsync(cancellationToken);
        return job.ToResponse(language);
    }
}
