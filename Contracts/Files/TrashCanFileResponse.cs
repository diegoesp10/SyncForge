namespace Contracts.Files;

public sealed record TrashCanFileResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    string Status,
    DateTimeOffset UploadedAt,
    DateTimeOffset? ProcessedAt,
    int? Progress,
    string? Error,
    DateTimeOffset MovedAt,
    DateTimeOffset PurgeAt);
