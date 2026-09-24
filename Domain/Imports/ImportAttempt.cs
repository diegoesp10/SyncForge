using Domain.Imports.Enums;

namespace Domain.Imports;

public sealed class ImportAttempt
{
    private ImportAttempt() { } // EF Core

    internal ImportAttempt(int number, DateTime startedAt)
    {
        Number = number;
        StartedAt = startedAt;
    }

    public int Id { get; private set; }
    public int ImportJobId { get; private set; }
    public int Number { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public ImportFailureCode? FailureCode { get; private set; }

    internal void Complete(DateTime finishedAt) => FinishedAt = finishedAt;

    internal void Fail(ImportFailureCode failureCode, DateTime finishedAt)
    {
        FailureCode = failureCode;
        FinishedAt = finishedAt;
    }
}
