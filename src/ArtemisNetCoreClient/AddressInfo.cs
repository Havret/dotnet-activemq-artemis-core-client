using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public class AddressInfo
{
    public string Name { get; init; }
    public IReadOnlyList<string> QueueNames { get; init; }
    public IReadOnlyList<RoutingType> RoutingTypes { get; init; }
}