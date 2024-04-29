using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Session(Connection connection, ILoggerFactory loggerFactory) : ISession, IChannel
{
    private readonly object _emptyResult = new();

    private readonly ConcurrentDictionary<long, TaskCompletionSource<object>> _completionSources = new();
    private readonly ConcurrentDictionary<long, Consumer> _consumers = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<Session> _logger = loggerFactory.CreateLogger<Session>();

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
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(createAddressMessage, ChannelId);
            await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources.TryRemove(-1, out _);
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
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
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
            _completionSources.TryRemove(-1, out _);
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

    private static RoutingType[] GetRoutingTypes(in SessionBindingQueryResponseMessage sessionBindingQueryResponseMessage)
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
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
             await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
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
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
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
            _completionSources.TryRemove(-1, out _);
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
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
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
            _completionSources.TryRemove(-1, out _);
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
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
            await tcs.Task;
            _consumers.TryRemove(consumerId, out _);
        }
        catch (Exception)
        {
            _completionSources.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public ValueTask<IProducer> CreateProducerAsync(ProducerConfiguration producerConfiguration, CancellationToken cancellationToken)
    {
        var request = new CreateProducerMessage
        {
            Id = 0,
            Address = producerConfiguration.Address
        };
        connection.Send(request, ChannelId);
        return ValueTask.FromResult<IProducer>(new Producer(this)
        {
            ProducerId = request.Id
        });
    }
    
    internal ValueTask RemoveProducerAsync(int producerId)
    {
        var request = new RemoveProducerMessage
        {
            Id = producerId,
        };
        connection.Send(request, ChannelId);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await CloseAsync();
        connection.RemoveChannel(ChannelId);
    }

    private async ValueTask StopAsync()
    {
        var sessionStop = new SessionStop();
        try
        {
            await _lock.WaitAsync();
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(sessionStop, ChannelId);
            await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async ValueTask CloseAsync()
    {
        var sessionCloseMessage2 = new SessionCloseMessage();
        try
        {
            await _lock.WaitAsync();
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(sessionCloseMessage2, ChannelId);
            await tcs.Task;
        }
        catch (Exception)
        {
            _completionSources.TryRemove(-1, out _);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Start()
    {
        var sessionStart = new SessionStart();
        connection.Send(sessionStart, ChannelId);
    }

    public void OnPacket(in InboundPacket packet)
    {
        switch (packet.PacketType)
        {
            case PacketType.NullResponse:
            {
                var nullResponse = new NullResponse(packet.Payload);
                if (_completionSources.TryRemove(nullResponse.CorrelationId, out var tcs))
                {
                    tcs.TrySetResult(_emptyResult);
                }

                break;
            }
            case PacketType.SessionBindingQueryResponseMessage:
            {
                var response = new SessionBindingQueryResponseMessage(packet.Payload);
                if (_completionSources.TryRemove(-1, out var tcs))
                {
                    var result = response.Exists
                        ? new AddressInfo { QueueNames = response.QueueNames, RoutingTypes = GetRoutingTypes(response) }
                        : _emptyResult;
                    tcs.TrySetResult(result);
                }

                break;
            }
            case PacketType.SessionQueueQueryResponseMessage:
            {
                var response = new SessionQueueQueryResponseMessage(packet.Payload);
                if (_completionSources.TryRemove(-1, out var tcs))
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