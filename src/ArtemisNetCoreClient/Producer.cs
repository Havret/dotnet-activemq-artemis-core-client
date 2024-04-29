using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

internal class Producer(Session session) : IProducer
{
    public required int ProducerId { get; init; }
    
    public ValueTask DisposeAsync()
    {
        return session.RemoveProducerAsync(ProducerId);
    }

    public async ValueTask SendMessageAsync(Message message, CancellationToken cancellationToken)
    {
        await session.SendMessageAsync(message, ProducerId, cancellationToken);
    }
}