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

    public async ValueTask SendMessageAsync(Message message, CancellationToken cancellationToken)
    {
        message.Address = Address;
        await session.SendMessageAsync(message: message, producerId: ProducerId, cancellationToken: cancellationToken);
    }
}