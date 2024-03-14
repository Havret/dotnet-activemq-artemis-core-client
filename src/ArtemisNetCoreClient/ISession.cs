using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public interface ISession : IAsyncDisposable;

internal class Session : ISession
{
    private readonly Transport _transport;

    private ConcurrentDictionary<long, TaskCompletionSource<Packet>> _completionSources = new();

    public Session(Transport transport)
    {
        _transport = transport;

        _ = Task.Run(async () =>
        {
            while (true)
            {
                var packet = await _transport.ReceiveAsync(default);
                
            }
        });
    }
    
    public async ValueTask DisposeAsync()
    {
        
        await _transport.SendAsync(new SessionStop(), ChannelId, default);
        
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    public long ChannelId { get; init; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _transport.SendAsync(new SessionStart(), ChannelId, cancellationToken);
    }
}