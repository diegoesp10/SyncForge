namespace Contracts.Files;

public sealed record FileResultResponse(
    Guid FileId,
    IReadOnlyDictionary<string, object?> Summary,
    FilePreviewResponse Preview,
    IReadOnlyList<string>? Warnings);
