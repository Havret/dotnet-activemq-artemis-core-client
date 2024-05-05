namespace ActiveMQ.Artemis.Core.Client;

public class QueueConfiguration
{
    public required string Address { get; init; }
    public required string Name { get; init; }
    public required RoutingType RoutingType { get; init; }
}