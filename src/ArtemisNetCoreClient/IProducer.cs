using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public interface IProducer : IAsyncDisposable
{
    ValueTask SendMessage(Message message, CancellationToken cancellationToken);
}

internal class Producer(Session session) : IProducer
{
    public required int ProducerId { get; init; }
    
    public async ValueTask DisposeAsync()
    {
        var request = new RemoveProducerMessage
        {
            Id = ProducerId
        };
        await session.SendAsync(request, default);
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