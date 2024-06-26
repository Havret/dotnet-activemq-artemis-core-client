using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Exceptions;
using ActiveMQ.Artemis.Core.Client.Framing;
using ActiveMQ.Artemis.Core.Client.Framing.Incoming;
using ActiveMQ.Artemis.Core.Client.Framing.Outgoing;
using ActiveMQ.Artemis.Core.Client.InternalUtilities;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Session(Connection connection, ILoggerFactory loggerFactory) : ISession, IChannel
{
    private readonly object _emptyResult = new();

    private readonly ConcurrentDictionary<long, TaskCompletionSource<object>> _completionSources = new();
    private readonly ConcurrentDictionary<long, Consumer> _consumers = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<Session> _logger = loggerFactory.CreateLogger<Session>();
    private readonly IdGenerator _correlationIdGenerator = new(0);
    private readonly IdGenerator _consumerIdGenerator = new(0);
    private readonly IdGenerator _producerIdGenerator = new(1);

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
        
        await _lock.WaitAsync(cancellationToken);
        try
        {
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

        await _lock.WaitAsync(cancellationToken);
        try
        {
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
            Durable = true,
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

        await _lock.WaitAsync(cancellationToken);
        try
        {
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
        
        await _lock.WaitAsync(cancellationToken);
        try
        {
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
    
    public async Task DeleteQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        var request = new SessionDeleteQueueMessage
        {
            QueueName = queueName
        };
        
        await _lock.WaitAsync(cancellationToken);
        try
        {
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

    public async Task<IConsumer> CreateConsumerAsync(ConsumerConfiguration consumerConfiguration, CancellationToken cancellationToken)
    {
        var generateId = _consumerIdGenerator.GenerateId();
        var request = new SessionCreateConsumerMessage
        {
            Id = generateId,
            QueueName = consumerConfiguration.QueueName,
            Priority = 0,
            BrowseOnly = false,
            RequiresResponse = true,
            FilterString = null
        };
        
        await _lock.WaitAsync(cancellationToken);
        try
        {
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
                _consumers.TryAdd(request.Id, consumer);
                
                consumer.Start();
                
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
        
        await _lock.WaitAsync();
        try
        {
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
            Id = (int) _producerIdGenerator.GenerateId(),
            Address = producerConfiguration.Address
        };
        connection.Send(request, ChannelId);
        return ValueTask.FromResult<IProducer>(new Producer(this)
        {
            ProducerId = request.Id,
            Address = producerConfiguration.Address,
            RoutingType = producerConfiguration.RoutingType
        });
    }

    public ValueTask<IAnonymousProducer> CreateAnonymousProducerAsync(CancellationToken cancellationToken)
    {
        var request = new CreateProducerMessage
        {
            Id = (int) _producerIdGenerator.GenerateId(),
            Address = null
        };
        connection.Send(request, ChannelId);
        return ValueTask.FromResult<IAnonymousProducer>(new AnonymousProducer(this)
        {
            ProducerId = request.Id
        });
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        var request = new SessionCommitMessage
        {
            CorrelationId = _correlationIdGenerator.GenerateId()
        };
        try
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(request.CorrelationId, tcs);
            connection.Send(request, ChannelId);
            await tcs.Task.WaitAsync(cancellationToken);
        }
        catch (Exception)
        {
            _completionSources.TryRemove(request.CorrelationId, out _);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        var request = new RollbackMessage
        {
            ConsiderLastMessageAsDelivered = false
        };

        await _lock.WaitAsync(cancellationToken);
        try
        {
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

    internal ValueTask RemoveProducerAsync(int producerId)
    {
        var request = new RemoveProducerMessage
        {
            Id = producerId,
        };
        connection.Send(request, ChannelId);
        return ValueTask.CompletedTask;
    }
    
    internal void SendMessage(Message message, string address, RoutingType? routingType, int producerId)
    {
        var request = new SessionSendMessage
        {
            Message = message,
            ProducerId = producerId,
            RequiresResponse = false,
            CorrelationId = -1,
            Address = address,
            RoutingType = routingType
        };
        connection.Send(request, ChannelId);
    }
    
    internal async Task SendMessageAsync(Message message, string address, RoutingType? routingType, int producerId, CancellationToken cancellationToken)
    {
        var request = new SessionSendMessage
        {
            Message = message,
            ProducerId = producerId,
            RequiresResponse = true,
            CorrelationId = _correlationIdGenerator.GenerateId(),
            Address = address,
            RoutingType = routingType
        };
        try
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(request.CorrelationId, tcs);
            connection.Send(request, ChannelId);
            await tcs.Task.WaitAsync(cancellationToken);
        }
        catch (Exception)
        {
            _completionSources.TryRemove(request.CorrelationId, out _);
            throw;
        }
    }
    
    internal void SendConsumerCredits(long consumerId, int credit)
    {
        var request = new SessionConsumerFlowCreditMessage
        {
            ConsumerId = consumerId,
            Credits = credit
        };
        connection.Send(request, ChannelId);
    }
    
    internal async ValueTask IndividualAcknowledgeAsync(MessageDelivery messageDelivery, CancellationToken cancellationToken)
    {
        var request = new SessionIndividualAcknowledgeMessage
        {
            ConsumerId = messageDelivery.ConsumerId,
            MessageId = messageDelivery.MessageId,
            RequiresResponse = true,
        };
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
            await tcs.Task.WaitAsync(cancellationToken);
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
    
    public async ValueTask AcknowledgeAsync(MessageDelivery messageDelivery, CancellationToken cancellationToken)
    {
        var request = new SessionAcknowledgeMessage
        {
            ConsumerId = messageDelivery.ConsumerId,
            MessageId = messageDelivery.MessageId,
            RequiresResponse = true,
        };
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(-1, tcs);
            connection.Send(request, ChannelId);
            await tcs.Task.WaitAsync(cancellationToken);
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
    
    public void Acknowledge(in MessageDelivery messageDelivery)
    {
        var request = new SessionAcknowledgeMessage
        {
            ConsumerId = messageDelivery.ConsumerId,
            MessageId = messageDelivery.MessageId,
            RequiresResponse = false,
        };
        connection.Send(request, ChannelId);
    }
    
    public void IndividualAcknowledge(in MessageDelivery messageDelivery)
    {
        var request = new SessionIndividualAcknowledgeMessage
        {
            ConsumerId = messageDelivery.ConsumerId,
            MessageId = messageDelivery.MessageId,
            RequiresResponse = false,
        };
        connection.Send(request, ChannelId);
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
        
        await _lock.WaitAsync();
        try
        {
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
        
        await _lock.WaitAsync();
        try
        {
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
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
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
                    var result = response.Exists
                        ? new QueueInfo
                        {
                            AddressName = response.Address!,
                            QueueName = response.Name!,
                            RoutingType = response.RoutingType,
                            MessageCount = response.MessageCount
                        }
                        : _emptyResult;
                    tcs.TrySetResult(result);
                }

                break;
            }
            case PacketType.SessionReceiveMessage:
            {
                var message = new SessionReceiveMessage(packet.Payload);
                if (_consumers.TryGetValue(message.ConsumerId, out var consumer))
                {
                    consumer.OnMessage(message.Message);
                }

                break;
            }
            case PacketType.Exception:
            {
                var message = new ActiveMQExceptionMessage(packet.Payload);
                if (_completionSources.TryRemove(message.CorrelationId, out var tcs))
                {
                    var exceptionType = (ActiveMQExceptionType) message.Code;
                    var activeMQException = exceptionType switch
                    {
                        ActiveMQExceptionType.QueueDoesNotExist => new ActiveMQNonExistentQueueException(),
                        _ => new ActiveMQException(exceptionType, message.Message ?? "ActiveMQ Exception with no message received")
                    };

                    tcs.TrySetException(activeMQException);
                }
                else
                {
                    _logger.LogError("Received exception message with code {Code} and message {Message}", message.Code, message.Message);
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