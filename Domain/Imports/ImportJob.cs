using Domain.Imports.Enums;
using Domain.Resources;

namespace Domain.Imports;

public sealed class ImportJob
{
    private ImportJob() { } // EF Core

    public ImportJob(string sourceSystem, string fileName, ImportFormat format, string language)
    {
        SourceSystem = Required(sourceSystem, nameof(sourceSystem), 200, language);
        FileName = Required(fileName, nameof(fileName), 260, language);
        if (!Enum.IsDefined(format))
            throw new ArgumentOutOfRangeException(nameof(format), ErrorMessages.Get(ErrorCode.InvalidFormat, language));

        Format = format;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string SourceSystem { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public ImportFormat Format { get; private set; }
    public ImportStatus Status { get; private set; } = ImportStatus.Pending;
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int AttemptCount { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Start(string language)
    {
        RequireStatus(ImportStatus.Pending, language);
        Status = ImportStatus.Processing;
        StartedAt = DateTime.UtcNow;
        FinishedAt = null;
        ErrorMessage = null;
        AttemptCount++;
    }

    public void Complete(string language)
    {
        RequireStatus(ImportStatus.Processing, language);
        Status = ImportStatus.Completed;
        FinishedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage, string language)
    {
        RequireStatus(ImportStatus.Processing, language);
        ErrorMessage = Required(errorMessage, nameof(errorMessage), 2000, language);
        Status = ImportStatus.Failed;
        FinishedAt = DateTime.UtcNow;
    }

    public void Retry(string language)
    {
        RequireStatus(ImportStatus.Failed, language);
        Status = ImportStatus.Pending;
        StartedAt = null;
        FinishedAt = null;
        ErrorMessage = null;
    }

    private void RequireStatus(ImportStatus expected, string language)
    {
        if (Status != expected)
            throw new InvalidOperationException(ErrorMessages.Get(
                ErrorCode.InvalidTransition, language, Id,
                ErrorMessages.Status(expected, language),
                ErrorMessages.Status(Status, language)));
    }

    private static string Required(string value, string name, int maxLength, string language)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.RequiredValue, language, name), name);

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.ValueTooLong, language, name, maxLength), name);

        return trimmed;
    }
}
