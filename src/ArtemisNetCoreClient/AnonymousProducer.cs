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
        session.SendMessage(message: message, address: address, routingType: routingType, producerId: ProducerId);
    }

    public Task SendMessageAsync(string address, RoutingType? routingType, Message message, CancellationToken cancellationToken = default)
    {
        return session.SendMessageAsync(message: message,
            address: address,
            routingType: routingType,
            producerId: ProducerId,
            cancellationToken: cancellationToken
        );
    }
}