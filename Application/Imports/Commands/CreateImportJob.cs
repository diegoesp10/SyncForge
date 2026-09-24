using Contracts.Imports;
using Domain.Imports;
using Domain.Imports.Enums;

namespace Application.Imports.Commands;

public sealed record CreateImportJobCommand(string SourceSystem, string FileName, ImportFormat Format);

public sealed class CreateImportJobHandler(IImportJobRepository repository)
{
    public async Task<ImportJobResponse> HandleAsync(CreateImportJobCommand command, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(command, nameof(command), language);
        var job = new ImportJob(command.SourceSystem, command.FileName, command.Format, language);
        await repository.AddAsync(job, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return job.ToResponse();
    }
}
