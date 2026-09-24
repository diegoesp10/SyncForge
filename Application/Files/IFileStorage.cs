namespace Application.Files;

public interface IFileStorage
{
    Task SaveAsync(Guid id, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
