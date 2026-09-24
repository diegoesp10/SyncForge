using System.Threading.Channels;
using Application.Files;

namespace Infrastructure.Files;

public sealed class FileWorkQueue : IFileWorkQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public ValueTask EnqueueAsync(Guid id, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(id, cancellationToken);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
