namespace ActiveMQ.Artemis.Core.Client;

internal class AnonymousProducer(Session session) : IAnonymousProducer
{
    public required int ProducerId { get; init; }
    
    public ValueTask DisposeAsync()
    {
        return session.RemoveProducerAsync(ProducerId);
    }

    public void SendMessage(string address, RoutingType? routingType, Message message)
    {
        message.Address = address;
        if (routingType != null)
        {
            message.RoutingType = routingType;
        }

        session.SendMessage(message: message, producerId: ProducerId);
    }

    public async ValueTask SendMessageAsync(string address, RoutingType? routingType, Message message, CancellationToken cancellationToken = default)
    {
        message.Address = address;
        if (routingType != null)
        {
            message.RoutingType = routingType;
        }

        await session.SendMessageAsync(message: message, producerId: ProducerId, cancellationToken: cancellationToken);
    }
}