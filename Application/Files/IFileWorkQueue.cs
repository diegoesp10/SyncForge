namespace Application.Files;

public interface IFileWorkQueue
{
    ValueTask EnqueueAsync(Guid id, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default);
}
