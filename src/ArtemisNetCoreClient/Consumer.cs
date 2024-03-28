using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

internal class Consumer(Session session) : IConsumer
{
    public required long ConsumerId { get; init; }
    
    public async ValueTask DisposeAsync()
    {
        var request = new SessionConsumerCloseMessage
        {
            ConsumerId = ConsumerId
        };
        _ = await session.SendBlockingAsync<SessionConsumerCloseMessage, NullResponse>(request, default);
    }
}