namespace ActiveMQ.Artemis.Core.Client;

public interface IProducer : IAsyncDisposable
{
    ValueTask SendMessageAsync(Message message, CancellationToken cancellationToken = default);
}