namespace ActiveMQ.Artemis.Core.Client;

public class ProducerConfiguration
{
    public required string Address { get; init; }
    public RoutingType? RoutingType { get; init; }
}