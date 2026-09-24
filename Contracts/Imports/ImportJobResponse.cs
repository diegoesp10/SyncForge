namespace Contracts.Imports;

public sealed record ImportJobResponse(
    int Id,
    string SourceSystem,
    string FileName,
    string Format,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    string? ErrorMessage,
    int AttemptCount);
