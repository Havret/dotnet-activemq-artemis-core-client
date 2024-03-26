using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public interface ISession : IAsyncDisposable
{
    Task CreateAddress(string address, IEnumerable<RoutingType> routingTypes, CancellationToken cancellationToken);
    Task<AddressInfo?> GetAddressInfo(string address, CancellationToken cancellationToken);
    Task CreateQueue(QueueConfiguration queueConfiguration, CancellationToken cancellationToken);
    Task<QueueInfo?> GetQueueInfo(string queueName, CancellationToken cancellationToken);
}