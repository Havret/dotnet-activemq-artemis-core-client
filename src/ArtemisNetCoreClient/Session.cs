using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Session : ISession, IChannel
{
    private readonly Connection _connection;
    private readonly Transport _transport;

    private readonly ConcurrentDictionary<long, TaskCompletionSource<Packet>> _completionSources = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource<IIncomingPacket>> _completionSources2 = new();
    private readonly ConcurrentDictionary<long, Consumer> _consumers = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<Session> _logger;

    public Session(Transport transport, ILoggerFactory loggerFactory)
    {
        _transport = transport;
        var logger = loggerFactory.CreateLogger<Session>();

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
                    else if (packet is SessionReceiveMessage { } sessionReceiveMessage)
                    {
                        if (_consumers.TryGetValue(sessionReceiveMessage.ConsumerId, out var consumer))
                        {
                            consumer.OnMessage(sessionReceiveMessage.Message);
                        }
                    }
                    else
                    {
                        // TODO: Handle
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in packet processing or network communication");
                    // TODO: Handle exception
                }
            }
        });
    }

    public Session(Connection connection, ILoggerFactory loggerFactory)
    {
        _connection = connection;
        _logger = loggerFactory.CreateLogger<Session>();
    }

    public required long ChannelId { get; init; }
    public required int ServerVersion { get; init; }

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

    public async Task<AddressInfo?> GetAddressInfo(string address, CancellationToken cancellationToken)
    {
        var request = new SessionBindingQueryMessage
        {
            Address = address
        };
        var response = await SendBlockingAsync<SessionBindingQueryMessage, SessionBindingQueryResponseMessageV5>(request, cancellationToken);

        if (response.Exists)
        {
            return new AddressInfo
            {
                Name = address,
                QueueNames = response.QueueNames,
                RoutingTypes = GetRoutingTypes(response).ToArray(),
            };
        }

        return null;
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
            RoutingType = queueConfiguration.RoutingType,
            MaxConsumers = -1
        };
        _ = await SendBlockingAsync<CreateQueueMessageV2, NullResponse>(createQueueMessage, cancellationToken);
    }

    public async Task<QueueInfo?> GetQueueInfo(string queueName, CancellationToken cancellationToken)
    {
        var request = new SessionQueueQueryMessage
        {
            QueueName = queueName
        };
        var response = await SendBlockingAsync<SessionQueueQueryMessage, SessionQueueQueryResponseMessageV3>(request, cancellationToken);

        if (response.Exists)
        {
            return new QueueInfo
            {
                QueueName = response.Name ?? queueName,
                RoutingType = response.RoutingType,
                AddressName = response.Address ?? string.Empty,
            };
        }

        return null;
    }

    public async Task<IConsumer> CreateConsumerAsync(ConsumerConfiguration consumerConfiguration, CancellationToken cancellationToken)
    {
        var request = new SessionCreateConsumerMessage
        {
            Id = 0,
            QueueName = consumerConfiguration.QueueName,
            Priority = 0,
            BrowseOnly = false,
            RequiresResponse = true
        };
        _ = await SendBlockingAsync<SessionCreateConsumerMessage, SessionQueueQueryResponseMessageV3>(request, cancellationToken);
        var consumer = new Consumer(this)
        {
            ConsumerId = request.Id
        };

        // TODO: We should remove consumer from this dictionary on dispose
        _consumers.TryAdd(request.Id, consumer);
        return consumer;
    }

    public async Task<IProducer> CreateProducerAsync(ProducerConfiguration producerConfiguration, CancellationToken cancellationToken)
    {
        var request = new CreateProducerMessage
        {
            Id = 0,
            Address = producerConfiguration.Address
        };
        await _transport.SendAsync(request, ChannelId, cancellationToken);
        return new Producer(this)
        {
            ProducerId = request.Id
        };
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
        // _ = await SendBlockingAsync<SessionStop, NullResponse>(new SessionStop(), default);
        // _ = await SendBlockingAsync<SessionCloseMessage, NullResponse>(new SessionCloseMessage(), default);
        // await _transport.DisposeAsync().ConfigureAwait(false);
    }

    internal async Task<TResponse> SendBlockingAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : Packet
    {
        var tcs = new TaskCompletionSource<Packet>();

        // TODO: Handle scenario when we cannot add request for this CorrelationId, because there is already another pending request
        // _ = _completionSources.TryAdd(request.CorrelationId, tcs);

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

    internal async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : Packet
    {
        await _transport.SendAsync(request, ChannelId, cancellationToken);
    }

    public void StartAsync2()
    {
        var sessionStart = new SessionStart2();
        _connection.Send(ref sessionStart, ChannelId);
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _transport.SendAsync(new SessionStart(), ChannelId, cancellationToken);
    }

    public void OnPacket(ref readonly InboundPacket packet)
    {
        switch (packet.PacketType)
        {
            case PacketType.NullResponse:
                var nullResponse = new NullResponse2(packet.Payload);
                if (_completionSources2.TryRemove(nullResponse.CorrelationId, out var tcs))
                {
                    tcs.TrySetResult(nullResponse);
                }
                break;
            default:
                _logger.LogWarning("Received unexpected packet type {PacketType}", packet.PacketType);
                break;
        }
    }
}