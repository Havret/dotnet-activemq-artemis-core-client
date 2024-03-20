using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

internal class Session : ISession
{
    private readonly Transport _transport;

    private readonly ConcurrentDictionary<long, TaskCompletionSource<Packet>> _completionSources = new();

    public Session(Transport transport)
    {
        _transport = transport;

        // TODO: Clean up while loop on close
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var packet = await _transport.ReceiveAsync(default);
                    if (packet is { IsResponse: true } && _completionSources.TryRemove(packet.CorrelationId, out var tcs))
                    {
                        tcs.TrySetResult(packet);
                    }
                    else
                    {
                        // TODO: Handle
                    }
                }
                catch (Exception e)
                {
                    // TODO: Handle exception
                    Console.WriteLine(e);
                }
            }
        });
    }
    
    public async Task CreateAddress(string address, IEnumerable<RoutingType> routingTypes, bool autoCreated, CancellationToken cancellationToken)
    {
        var createAddressMessage = new CreateAddressMessage
        {
            Address = address,
            RoutingTypes = routingTypes.ToArray(),
            AutoCreated = autoCreated,
            RequiresResponse = true
        };
        _ = await SendBlockingAsync<CreateAddressMessage, NullResponse>(createAddressMessage, 11, cancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        _ = await SendBlockingAsync<SessionStop, NullResponse>(new SessionStop(), ChannelId, default);
        _ = await SendBlockingAsync<SessionCloseMessage, NullResponse>(new SessionCloseMessage(),ChannelId, default);
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<TResponse> SendBlockingAsync<TRequest, TResponse>(TRequest request, long channelId, CancellationToken cancellationToken) where TRequest : Packet
    {
        var tcs = new TaskCompletionSource<Packet>();
        
        // TODO: Handle scenario when we cannot CorrelationId
        _ = _completionSources.TryAdd(request.CorrelationId, tcs);

        await _transport.SendAsync(request, channelId, cancellationToken);
        var responsePacket = await tcs.Task;
        if (responsePacket is TResponse response)
        {
            return response;
        }
        else
        {
            // TODO: Handle gracefully
            throw new ArgumentException($"Expected response {typeof(TResponse).Name} but got {responsePacket.GetType().Name}");
        }
    }

    public long ChannelId { get; init; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _transport.SendAsync(new SessionStart(), ChannelId, cancellationToken);
    }
}