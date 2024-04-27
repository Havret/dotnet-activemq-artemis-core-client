using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Session : ISession, IChannel
{
    private readonly Connection _connection;
    private readonly Transport _transport;

    private readonly object _emptyResult = new();

    private readonly ConcurrentDictionary<long, TaskCompletionSource<Packet>> _completionSources = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource<object>> _completionSources2 = new();
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

    public async Task CreateAddressAsync(string address, IEnumerable<RoutingType> routingTypes, CancellationToken cancellationToken)
    {
        var createAddressMessage = new CreateAddressMessage
        {
            Address = address,
            RoutingTypes = routingTypes.ToArray(),
            AutoCreated = false,
            RequiresResponse = true
        };
        try
        {
            await _lock.WaitAsync(cancellationToken);
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref createAddressMessage, ChannelId);
            await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<AddressInfo?> GetAddressInfoAsync(string address, CancellationToken cancellationToken)
    {
        var request = new SessionBindingQueryMessage
        {
            Address = address
        };

        try
        {
            await _lock.WaitAsync(cancellationToken);
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref request, ChannelId);
            var result = await tcs.Task;
            if (result is AddressInfo addressInfo)
            {
                return addressInfo;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static readonly RoutingType[] BothRoutingTypes = [RoutingType.Anycast, RoutingType.Multicast];
    private static readonly RoutingType[] AnycastRoutingType = [RoutingType.Anycast];
    private static readonly RoutingType[] MulticastRoutingType = [RoutingType.Multicast];

    private static RoutingType[] GetRoutingTypes(ref SessionBindingQueryResponseMessage sessionBindingQueryResponseMessage)
    {
        return sessionBindingQueryResponseMessage switch
        {
            { SupportsAnycast: true, SupportsMulticast: true } => BothRoutingTypes,
            { SupportsAnycast: true } => AnycastRoutingType,
            { SupportsMulticast: true } => MulticastRoutingType,
            _ => []
        };
    }

    public async Task CreateQueueAsync(QueueConfiguration queueConfiguration, CancellationToken cancellationToken)
    {
        var request = new CreateQueueMessage
        {
            Address = queueConfiguration.Address,
            QueueName = queueConfiguration.Name,
            FilterString = null,
            Durable = false,
            Temporary = false,
            RequiresResponse = true,
            AutoCreated = false,
            RoutingType = queueConfiguration.RoutingType,
            MaxConsumers = -1,
            PurgeOnNoConsumers = false,
            Exclusive = null,
            LastValue = null,
            LastValueKey = null,
            NonDestructive = null,
            ConsumersBeforeDispatch = null,
            DelayBeforeDispatch = null,
            GroupRebalance = null,
            GroupBuckets = null,
            AutoDelete = null,
            AutoDeleteDelay = null,
            AutoDeleteMessageCount = null,
            GroupFirstKey = null,
            RingSize = null,
            Enabled = null,
            GroupRebalancePauseDispatch = null
        };

        try
        {
            await _lock.WaitAsync(cancellationToken);
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref request, ChannelId);
             await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }

        // var createQueueMessage = new CreateQueueMessageV2
        // {
        //     RequiresResponse = true,
        //     Address = queueConfiguration.Address,
        //     QueueName = queueConfiguration.Name,
        //     RoutingType = queueConfiguration.RoutingType,
        //     MaxConsumers = -1
        // };
        // _ = await SendBlockingAsync<CreateQueueMessageV2, NullResponse>(createQueueMessage, cancellationToken);
    }

    public async Task<QueueInfo?> GetQueueInfoAsync(string queueName, CancellationToken cancellationToken)
    {
        var request = new SessionQueueQueryMessage
        {
            QueueName = queueName
        };
        
        try
        {
            await _lock.WaitAsync(cancellationToken);
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref request, ChannelId);
            var result = await tcs.Task;
            if (result is QueueInfo queueInfo)
            {
                return queueInfo;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IConsumer> CreateConsumerAsync(ConsumerConfiguration consumerConfiguration, CancellationToken cancellationToken)
    {
        var request = new SessionCreateConsumerMessage
        {
            Id = 0,
            QueueName = consumerConfiguration.QueueName,
            Priority = 0,
            BrowseOnly = false,
            RequiresResponse = true,
            FilterString = null
        };
        try
        {
            await _lock.WaitAsync(cancellationToken);
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref request, ChannelId);
            var result = await tcs.Task;
            if (result is QueueInfo)
            {
                var consumer = new Consumer(this)
                {
                    ConsumerId = request.Id
                };
                // TODO: We should remove consumer from this dictionary on dispose
                _consumers.TryAdd(request.Id, consumer);
                
                return consumer;
            }
            else
            {
                // TODO: Handle scneario when we cannot create consumer
                return null!;
            }
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    internal async Task CloseConsumer(long consumerId)
    {
        var request = new SessionConsumerCloseMessage
        {
            ConsumerId = consumerId
        };
        try
        {
            await _lock.WaitAsync();
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref request, ChannelId);
            await tcs.Task;
            _consumers.TryRemove(consumerId, out _);
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
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

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await CloseAsync();
        _connection.RemoveChannel(ChannelId);
    }

    private async ValueTask StopAsync()
    {
        var sessionStop = new SessionStop2();
        try
        {
            await _lock.WaitAsync();
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref sessionStop, ChannelId);
            await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async ValueTask CloseAsync()
    {
        var sessionCloseMessage2 = new SessionCloseMessage2();
        try
        {
            await _lock.WaitAsync();
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources2.TryAdd(-1, tcs);
            _connection.Send(ref sessionCloseMessage2, ChannelId);
            await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources2.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
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
            {
                var nullResponse = new NullResponse2(packet.Payload);
                if (_completionSources2.TryRemove(nullResponse.CorrelationId, out var tcs))
                {
                    tcs.TrySetResult(_emptyResult);
                }

                break;
            }
            case PacketType.SessionBindingQueryResponseMessage:
            {
                var response = new SessionBindingQueryResponseMessage(packet.Payload);
                if (_completionSources2.TryRemove(-1, out var tcs))
                {
                    var addressInfo = new AddressInfo
                    {
                        QueueNames = response.QueueNames,
                        RoutingTypes = GetRoutingTypes(ref response),
                    };
                    tcs.TrySetResult(addressInfo);
                }

                break;
            }
            case PacketType.SessionQueueQueryResponseMessage:
            {
                var response = new SessionQueueQueryResponseMessage(packet.Payload);
                if (_completionSources2.TryRemove(-1, out var tcs))
                {
                    var queueInfo = new QueueInfo
                    {
                        AddressName = response.Address!,
                        QueueName = response.Name!,
                        RoutingType = response.RoutingType,
                    };
                    tcs.TrySetResult(queueInfo);
                }

                break;
            }
            default:
            {
                _logger.LogWarning("Received unexpected packet type {PacketType}", packet.PacketType);
                break;
            }
        }
    }
}