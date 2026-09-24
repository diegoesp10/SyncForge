using Contracts.Files;
using Domain.Files;
using Domain.Resources;

namespace Application.Files;

public sealed class TrashCanService(
    IFileRepository files,
    ITrashCanRepository trashCan,
    IFileStorage storage,
    IFileWorkQueue queue,
    TimeProvider clock) : ITrashCanService
{
    public async Task<TrashCanFileResponse> MoveToTrashCanAsync(Guid id, string language, CancellationToken cancellationToken = default)
    {
        if (await trashCan.GetByFileIdAsync(id, cancellationToken) is not null)
            throw new InvalidOperationException(ErrorMessages.Get(ErrorCode.FileAlreadyInTrash, language, id));

        var file = await files.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.FileNotFound, language, id));
        file.MoveToTrashCan(language);

        var entry = new TrashCanEntry(file, clock.GetUtcNow());
        await trashCan.AddAsync(entry, cancellationToken);
        await trashCan.SaveChangesAsync(cancellationToken);
        return entry.ToTrashCanResponse(language);
    }

    public async Task<IReadOnlyList<TrashCanFileResponse>> ListAsync(string language, CancellationToken cancellationToken = default) =>
        (await trashCan.ListAsync(cancellationToken)).Select(entry => entry.ToTrashCanResponse(language)).ToArray();

    public async Task<TrashCanFileResponse> GetAsync(Guid id, string language, CancellationToken cancellationToken = default) =>
        (await RequireAsync(id, language, cancellationToken)).ToTrashCanResponse(language);

    public async Task<FileResultResponse> GetResultAsync(Guid id, string language, CancellationToken cancellationToken = default) =>
        (await RequireAsync(id, language, cancellationToken)).File.ToResultResponse(language);

    public async Task<FileItemResponse> RestoreAsync(Guid id, string language, CancellationToken cancellationToken = default)
    {
        var entry = await RequireAsync(id, language, cancellationToken);
        entry.EnsureRestorable(clock.GetUtcNow(), language);
        trashCan.Remove(entry);
        await trashCan.SaveChangesAsync(cancellationToken);

        if (entry.File.Status == FileStatus.Pending)
            await queue.EnqueueAsync(id, CancellationToken.None);
        return entry.File.ToResponse(language);
    }

    public async Task PurgeAsync(Guid id, string language, CancellationToken cancellationToken = default)
    {
        var entry = await RequireAsync(id, language, cancellationToken);
        entry.BeginPurge(clock.GetUtcNow());
        await trashCan.SaveChangesAsync(cancellationToken);

        await storage.DeleteAsync(id, cancellationToken);
        files.Remove(entry.File);
        await files.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<Guid>> ListExpiredIdsAsync(CancellationToken cancellationToken = default) =>
        trashCan.ListExpiredIdsAsync(clock.GetUtcNow(), cancellationToken);

    private async Task<TrashCanEntry> RequireAsync(Guid id, string language, CancellationToken cancellationToken) =>
        await trashCan.GetByFileIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.TrashFileNotFound, language, id));
}
