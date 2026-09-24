namespace Contracts.Imports;

public sealed record ImportJobResponse(
    int Id,
    string SourceSystem,
    string FileName,
    string StoredFileKey,
    string Format,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    string? FailureCode,
    string? ErrorMessage,
    int AttemptCount,
    IReadOnlyList<ImportAttemptResponse> Attempts);
