using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public interface ISession : IAsyncDisposable
{
    public Task CreateAddress(string address, IEnumerable<RoutingType> routingTypes, bool autoCreated, CancellationToken cancellationToken);
}