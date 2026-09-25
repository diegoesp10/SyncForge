using Domain.Resources;

namespace Domain.Files;

public sealed class StoredFile
{
    private StoredFile() { } // EF Core

    public StoredFile(Guid id, string fileName, string contentType, long size, string language)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidRequest, language), nameof(id));
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), ErrorMessages.Get(ErrorCode.EmptyFile, language));

        Id = id;
        FileName = DomainValidation.Required(fileName, nameof(fileName), 260, language);
        ContentType = DomainValidation.Required(contentType, nameof(contentType), 200, language);
        Size = size;
        UploadedAt = DateTimeOffset.UtcNow;
        Version = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long Size { get; private set; }
    public FileStatus Status { get; private set; } = FileStatus.Pending;
    public DateTimeOffset UploadedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int? Progress { get; private set; }
    public FileFailureCode? FailureCode { get; private set; }
    public string? ResultJson { get; private set; }
    public Guid Version { get; private set; }

    public void Start(string language)
    {
        RequireStatus(FileStatus.Pending, language);
        Status = FileStatus.Processing;
        Progress = 0;
        Version = Guid.NewGuid();
    }

    public void Complete(string resultJson, string language)
    {
        RequireStatus(FileStatus.Processing, language);
        ResultJson = resultJson;
        FailureCode = null;
        Status = FileStatus.Completed;
        Progress = null;
        ProcessedAt = DateTimeOffset.UtcNow;
        Version = Guid.NewGuid();
    }

    public void Fail(FileFailureCode failureCode, string language)
    {
        RequireStatus(FileStatus.Processing, language);
        FailureCode = failureCode;
        ResultJson = null;
        Status = FileStatus.Failed;
        Progress = null;
        ProcessedAt = DateTimeOffset.UtcNow;
        Version = Guid.NewGuid();
    }

    public void Reprocess(string language)
    {
        if (Status is not (FileStatus.Completed or FileStatus.Failed))
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.InvalidFileTransition, language));

        Status = FileStatus.Pending;
        ResultJson = null;
        FailureCode = null;
        Progress = null;
        ProcessedAt = null;
        Version = Guid.NewGuid();
    }

    public void MoveToTrashCan(string language)
    {
        if (Status == FileStatus.Processing)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.InvalidFileTransition, language));

        Version = Guid.NewGuid();
    }

    public void Rename(string? fileName, string language)
    {
        var name = DomainValidation.Required(fileName, nameof(fileName), 260, language);
        if (name is "." or ".." || name.EndsWith('.') || name.Any(character =>
                char.IsControl(character) || character is '<' or '>' or ':' or '"' or '/' or '\\' or '|' or '?' or '*'))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidFileName, language), nameof(fileName));

        var extension = Path.GetExtension(name);
        if (string.IsNullOrWhiteSpace(name[..^extension.Length].Trim('.')))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidFileName, language), nameof(fileName));
        if (!extension.Equals(Path.GetExtension(FileName), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.FileExtensionCannotChange, language), nameof(fileName));
        if (Status == FileStatus.Processing)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.InvalidFileTransition, language));
        if (name == FileName)
            return;

        FileName = name;
        Version = Guid.NewGuid();
    }

    public void RecoverInterrupted()
    {
        if (Status != FileStatus.Processing)
            return;

        Status = FileStatus.Pending;
        Progress = null;
        Version = Guid.NewGuid();
    }

    private void RequireStatus(FileStatus expected, string language)
    {
        if (Status != expected)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.InvalidFileTransition, language));
    }
}
