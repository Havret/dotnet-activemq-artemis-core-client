using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// ActiveQ Artemis session. A session is a context for producing and consuming messages. It is also used to create queues and addresses.
/// </summary>
public interface ISession : IAsyncDisposable
{
    /// <summary>
    /// Create an address with the given routing types if it does not already exist.
    /// </summary>
    Task CreateAddress(string address, IEnumerable<RoutingType> routingTypes, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get information about an address.
    /// </summary>
    Task<AddressInfo?> GetAddressInfo(string address, CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a queue with the given configuration if it does not already exist.
    /// </summary>
    Task CreateQueue(QueueConfiguration queueConfiguration, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get information about a queue.
    /// </summary>
    Task<QueueInfo?> GetQueueInfo(string queueName, CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a consumer with the given configuration.
    /// </summary>
    Task<IConsumer> CreateConsumerAsync(ConsumerConfiguration consumerConfiguration, CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a producer with the given configuration.
    /// </summary>
    Task<IProducer> CreateProducerAsync(ProducerConfiguration producerConfiguration, CancellationToken cancellationToken);
}