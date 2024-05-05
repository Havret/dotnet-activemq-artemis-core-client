namespace ActiveMQ.Artemis.Core.Client;

public class QueueInfo
{
    public required string QueueName { get; init; }
    public required string AddressName { get; init; }
    public required RoutingType RoutingType { get; init; }
}