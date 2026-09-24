using Contracts.Files;
using Domain.Files;
using Domain.Resources;

namespace Application.Files;

internal static class FileMapping
{
    public static FileItemResponse ToResponse(this StoredFile file, string language) => new(
        file.Id,
        file.FileName,
        file.ContentType,
        file.Size,
        file.Status.ToString(),
        file.UploadedAt,
        file.ProcessedAt,
        file.Progress,
        file.FailureCode is { } code ? ErrorMessages.FileFailure(code, file.FileName, language) : null);
}
