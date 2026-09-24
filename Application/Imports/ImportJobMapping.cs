using Contracts.Imports;
using Domain.Imports;

namespace Application.Imports;

internal static class ImportJobMapping
{
    public static ImportJobResponse ToResponse(this ImportJob job) => new(
        job.Id,
        job.SourceSystem,
        job.FileName,
        job.Format.ToString(),
        job.Status.ToString(),
        job.CreatedAt,
        job.StartedAt,
        job.FinishedAt,
        job.ErrorMessage,
        job.AttemptCount);
}
