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
    
    public async Task CreateAddress(string address, IEnumerable<RoutingType> routingTypes, CancellationToken cancellationToken)
    {
        var createAddressMessage = new CreateAddressMessage
        {
            Address = address,
            RoutingTypes = routingTypes.ToArray(),
            AutoCreated = false,
            RequiresResponse = true
        };
        _ = await SendBlockingAsync<CreateAddressMessage, NullResponse>(createAddressMessage, cancellationToken);
    }

    public async Task<AddressInfo> GetAddressInfo(string address, CancellationToken cancellationToken)
    {
        var request = new SessionBindingQueryMessage
        {
            Address = address
        };
        var response = await SendBlockingAsync<SessionBindingQueryMessage, SessionBindingQueryResponseMessageV5>(request, cancellationToken);
        
        return new AddressInfo
        {
            Name = address,
            QueueNames = response.QueueNames,
            RoutingTypes = GetRoutingTypes(response).ToArray(),
        };
    }

    private static IEnumerable<RoutingType> GetRoutingTypes(SessionBindingQueryResponseMessageV5 sessionBindingQueryResponseMessageV5)
    {
        if (sessionBindingQueryResponseMessageV5.SupportsAnycast)
        {
            yield return RoutingType.Anycast;
        }

        if (sessionBindingQueryResponseMessageV5.SupportsMulticast)
        {
            yield return RoutingType.Multicast;
        }
    }
    
    public async Task CreateQueue(QueueConfiguration queueConfiguration, CancellationToken cancellationToken)
    {
        var createQueueMessage = new CreateQueueMessageV2
        {
            RequiresResponse = true,
            Address = queueConfiguration.Address,
            QueueName = queueConfiguration.Name,
            RoutingType = queueConfiguration.RoutingType
        };
        _ = await SendBlockingAsync<CreateQueueMessageV2, NullResponse>(createQueueMessage, cancellationToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        _ = await SendBlockingAsync<SessionStop, NullResponse>(new SessionStop(), default);
        _ = await SendBlockingAsync<SessionCloseMessage, NullResponse>(new SessionCloseMessage(), default);
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<TResponse> SendBlockingAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : Packet
    {
        var tcs = new TaskCompletionSource<Packet>();
        
        // TODO: Handle scenario when we cannot add request for this CorrelationId, because there is already another pending request
        _ = _completionSources.TryAdd(request.CorrelationId, tcs);

        await _transport.SendAsync(request, ChannelId, cancellationToken);
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