using System.Text.Json;
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

    public static TrashCanFileResponse ToTrashCanResponse(this TrashCanEntry entry, string language) => new(
        entry.FileId,
        entry.File.FileName,
        entry.File.ContentType,
        entry.File.Size,
        entry.File.Status.ToString(),
        entry.File.UploadedAt,
        entry.File.ProcessedAt,
        entry.File.Progress,
        entry.File.FailureCode is { } code ? ErrorMessages.FileFailure(code, entry.File.FileName, language) : null,
        entry.MovedAt,
        entry.PurgeAt);

    public static FileResultResponse ToResultResponse(this StoredFile file, string language)
    {
        if (file.Status != FileStatus.Completed || file.ResultJson is null)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.FileResultUnavailable, language, file.Id));

        var result = JsonSerializer.Deserialize<FileResultResponse>(file.ResultJson)!;
        return result with
        {
            Warnings = result.Warnings?.Select(warning =>
                warning == nameof(ErrorCode.InvalidJsonPreviewWarning)
                    ? ErrorMessages.Get(ErrorCode.InvalidJsonPreviewWarning, language)
                    : warning).ToArray()
        };
    }
}
