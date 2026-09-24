using Domain.Imports.Enums;
using Domain.Resources;

namespace Domain.Imports;

public sealed class ImportJob
{
    private readonly List<ImportAttempt> _attempts = [];
    private ImportJob() { } // EF Core

    public ImportJob(string sourceSystem, string fileName, string storedFileKey, ImportFormat format, string language)
    {
        SourceSystem = DomainValidation.Required(sourceSystem, nameof(sourceSystem), 200, language);
        FileName = DomainValidation.Required(fileName, nameof(fileName), 260, language);
        StoredFileKey = DomainValidation.Required(storedFileKey, nameof(storedFileKey), 512, language);
        if (!Enum.IsDefined(format))
            throw new ArgumentOutOfRangeException(nameof(format), ErrorMessages.Get(ErrorCode.InvalidFormat, language));

        Format = format;
        CreatedAt = DateTime.UtcNow;
        Version = Guid.NewGuid();
    }

    public int Id { get; private set; }
    public string SourceSystem { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string StoredFileKey { get; private set; } = null!;
    public ImportFormat Format { get; private set; }
    public ImportStatus Status { get; private set; } = ImportStatus.Pending;
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public ImportFailureCode? FailureCode { get; private set; }
    public int AttemptCount { get; private set; }
    public Guid Version { get; private set; }
    public IReadOnlyCollection<ImportAttempt> Attempts => _attempts.AsReadOnly();

    public void Start(string language)
    {
        RequireStatus(ImportStatus.Pending, language);
        var now = DateTime.UtcNow;
        _attempts.Add(new ImportAttempt(AttemptCount + 1, now));
        AttemptCount++;
        Status = ImportStatus.Processing;
        StartedAt = now;
        FinishedAt = null;
        FailureCode = null;
        Version = Guid.NewGuid();
    }

    public void Complete(string language)
    {
        RequireStatus(ImportStatus.Processing, language);
        var now = DateTime.UtcNow;
        _attempts[^1].Complete(now);
        Status = ImportStatus.Completed;
        FinishedAt = now;
        Version = Guid.NewGuid();
    }

    public void Fail(ImportFailureCode failureCode, string language)
    {
        RequireStatus(ImportStatus.Processing, language);
        if (!Enum.IsDefined(failureCode))
            throw new ArgumentOutOfRangeException(nameof(failureCode), ErrorMessages.Get(ErrorCode.InvalidFailureCode, language));

        var now = DateTime.UtcNow;
        _attempts[^1].Fail(failureCode, now);
        FailureCode = failureCode;
        Status = ImportStatus.Failed;
        FinishedAt = now;
        Version = Guid.NewGuid();
    }

    public void Retry(string language)
    {
        RequireStatus(ImportStatus.Failed, language);
        Status = ImportStatus.Pending;
        StartedAt = null;
        FinishedAt = null;
        FailureCode = null;
        Version = Guid.NewGuid();
    }

    private void RequireStatus(ImportStatus expected, string language)
    {
        if (Status != expected)
            throw new InvalidOperationException(ErrorMessages.Get(
                ErrorCode.InvalidTransition, language, Id,
                ErrorMessages.Status(expected, language),
                ErrorMessages.Status(Status, language)));
    }
}
