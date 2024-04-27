using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

internal class Producer(Session session) : IProducer
{
    public required int ProducerId { get; init; }
    
    public ValueTask DisposeAsync()
    {
        return session.RemoveProducerAsync(ProducerId);
    }

    public async ValueTask SendMessage(Message message, CancellationToken cancellationToken)
    {
        await session.SendBlockingAsync<SessionSendMessageV3, NullResponse>(new SessionSendMessageV3
        {
            Message = message,
            ProducerId = ProducerId,
            RequiresResponse = true,
            CorrelationId = -4
        }, cancellationToken);
    }
}