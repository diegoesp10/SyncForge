namespace Contracts.Imports;

public sealed record ImportAttemptResponse(
    int Number,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string? FailureCode,
    string? ErrorMessage);
