namespace ActiveMQ.Artemis.Core.Client;

internal class Producer(Session session) : IProducer
{
    public required int ProducerId { get; init; }
    public required string Address { get; init; }
    public RoutingType? RoutingType { get; init; }

    public ValueTask DisposeAsync()
    {
        return session.RemoveProducerAsync(ProducerId);
    }

    public void SendMessage(Message message)
    {
        session.SendMessage(message: message, address: Address, routingType: RoutingType, producerId: ProducerId);
    }

    public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
    {
        return session.SendMessageAsync(message: message,
            address: Address,
            routingType: RoutingType,
            producerId: ProducerId,
            cancellationToken: cancellationToken
        );
    }
}