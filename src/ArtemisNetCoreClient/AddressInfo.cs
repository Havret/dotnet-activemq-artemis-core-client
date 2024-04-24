using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public class AddressInfo
{
    public required IReadOnlyList<string> QueueNames { get; init; }
    public required IReadOnlyList<RoutingType> RoutingTypes { get; init; }
}