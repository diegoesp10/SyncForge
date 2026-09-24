using System.Text.Json;
using Contracts.Files;
using Domain.Files;
using Domain.Resources;

namespace Application.Files;

public sealed class FileService(IFileRepository repository, IFileStorage storage, IFileWorkQueue queue) : IFileService
{
    public const int MaxFileSizeMb = 50;
    public const long MaxFileSizeBytes = MaxFileSizeMb * 1024L * 1024L;

    public async Task<FileItemResponse> UploadAsync(string fileName, string? contentType, long size, Stream content, string language, CancellationToken cancellationToken = default)
    {
        if (size > MaxFileSizeBytes)
            throw new FileTooLargeException(ErrorMessages.Get(ErrorCode.FileTooLarge, language, MaxFileSizeMb));

        var safeName = Path.GetFileName((fileName ?? string.Empty).Replace('\\', '/'));
        var file = new StoredFile(Guid.NewGuid(), safeName,
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            size, language);

        await storage.SaveAsync(file.Id, content, cancellationToken);
        try
        {
            await repository.AddAsync(file, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(file.Id, CancellationToken.None);
            throw;
        }

        await queue.EnqueueAsync(file.Id, CancellationToken.None);
        return file.ToResponse(language);
    }

    public async Task<IReadOnlyList<FileItemResponse>> ListAsync(string language, CancellationToken cancellationToken = default) =>
        (await repository.ListAsync(cancellationToken)).Select(file => file.ToResponse(language)).ToArray();

    public async Task<FileItemResponse> GetAsync(Guid id, string language, CancellationToken cancellationToken = default) =>
        (await RequireAsync(id, language, cancellationToken)).ToResponse(language);

    public async Task<FileResultResponse> GetResultAsync(Guid id, string language, CancellationToken cancellationToken = default)
    {
        var file = await RequireAsync(id, language, cancellationToken);
        if (file.Status != FileStatus.Completed || file.ResultJson is null)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.FileResultUnavailable, language, id));

        var result = JsonSerializer.Deserialize<FileResultResponse>(file.ResultJson)!;
        return result with
        {
            Warnings = result.Warnings?.Select(warning =>
                warning == nameof(ErrorCode.InvalidJsonPreviewWarning)
                    ? ErrorMessages.Get(ErrorCode.InvalidJsonPreviewWarning, language)
                    : warning).ToArray()
        };
    }

    public async Task<FileItemResponse> ReprocessAsync(Guid id, string language, CancellationToken cancellationToken = default)
    {
        var file = await RequireAsync(id, language, cancellationToken);
        file.Reprocess(language);
        await repository.SaveChangesAsync(cancellationToken);
        await queue.EnqueueAsync(id, cancellationToken);
        return file.ToResponse(language);
    }

    public async Task DeleteAsync(Guid id, string language, CancellationToken cancellationToken = default)
    {
        var file = await RequireAsync(id, language, cancellationToken);
        if (file.Status == FileStatus.Processing)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.InvalidFileTransition, language));

        repository.Remove(file);
        await repository.SaveChangesAsync(cancellationToken);
        await storage.DeleteAsync(id, cancellationToken);
    }

    private async Task<StoredFile> RequireAsync(Guid id, string language, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.FileNotFound, language, id));
}
