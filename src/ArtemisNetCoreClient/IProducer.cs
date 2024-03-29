using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

public interface IProducer : IAsyncDisposable
{
}

internal class Producer(Session session) : IProducer
{
    public required int ProducerId { get; init; }
    
    public async ValueTask DisposeAsync()
    {
        var request = new RemoveProducerMessage()
        {
            Id = ProducerId
        };
        await session.SendAsync(request, default);
    }
}