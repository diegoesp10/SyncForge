using Domain.Resources;

namespace Domain.Files;

public sealed class TrashCanEntry
{
    private TrashCanEntry() { } // EF Core

    public TrashCanEntry(StoredFile file, DateTimeOffset movedAt)
    {
        File = file;
        FileId = file.Id;
        MovedAt = movedAt;
        PurgeAt = movedAt.AddDays(30);
        Version = Guid.NewGuid();
    }

    public Guid FileId { get; private set; }
    public StoredFile File { get; private set; } = null!;
    public DateTimeOffset MovedAt { get; private set; }
    public DateTimeOffset PurgeAt { get; private set; }
    public DateTimeOffset? PurgeStartedAt { get; private set; }
    public Guid Version { get; private set; }

    public void BeginPurge(DateTimeOffset now)
    {
        if (PurgeStartedAt is not null)
            return;

        PurgeStartedAt = now;
        Version = Guid.NewGuid();
    }

    public void EnsureRestorable(DateTimeOffset now, string language)
    {
        if (PurgeStartedAt is not null || PurgeAt <= now)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.TrashRestoreUnavailable, language));
    }
}
