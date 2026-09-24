using Application.Files;

namespace Infrastructure.Files;

public sealed class LocalFileStorage(string rootPath) : IFileStorage
{
    private readonly string _rootPath = Path.GetFullPath(rootPath);

    public async Task SaveAsync(Guid id, Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootPath);
        var path = FilePath(id);
        var created = false;
        try
        {
            await using var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            created = true;
            await content.CopyToAsync(target, cancellationToken);
        }
        catch
        {
            if (created && File.Exists(path))
                File.Delete(path);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(FilePath(id), FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = FilePath(id);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string FilePath(Guid id) => Path.Combine(_rootPath, id.ToString("N"));
}
