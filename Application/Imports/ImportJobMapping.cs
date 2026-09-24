using Contracts.Imports;
using Domain.Imports;
using Domain.Resources;

namespace Application.Imports;

internal static class ImportJobMapping
{
    public static ImportJobResponse ToResponse(this ImportJob job, string language) => new(
        job.Id,
        job.SourceSystem,
        job.FileName,
        job.StoredFileKey,
        job.Format.ToString(),
        job.Status.ToString(),
        job.CreatedAt,
        job.StartedAt,
        job.FinishedAt,
        job.FailureCode?.ToString(),
        job.FailureCode is { } failureCode ? ErrorMessages.Failure(failureCode, language) : null,
        job.AttemptCount,
        job.Attempts.OrderBy(attempt => attempt.Number)
            .Select(attempt => new ImportAttemptResponse(
                attempt.Number,
                attempt.StartedAt,
                attempt.FinishedAt,
                attempt.FailureCode?.ToString(),
                attempt.FailureCode is { } code ? ErrorMessages.Failure(code, language) : null))
            .ToArray());
}
