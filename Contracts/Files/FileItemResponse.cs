namespace Contracts.Files;

public sealed record FileItemResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    string Status,
    DateTimeOffset UploadedAt,
    DateTimeOffset? ProcessedAt,
    int? Progress,
    string? Error);
