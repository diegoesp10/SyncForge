using System.Text.Json;

namespace Contracts.Files;

public sealed record FilePreviewResponse(
    string Kind,
    IReadOnlyList<string>? Columns,
    IReadOnlyList<IReadOnlyList<string?>>? Rows,
    string? Text,
    JsonElement? Json);
