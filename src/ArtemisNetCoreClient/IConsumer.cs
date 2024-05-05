namespace ActiveMQ.Artemis.Core.Client;

public interface IConsumer : IAsyncDisposable
{
    ValueTask<Message> ReceiveMessageAsync(CancellationToken cancellationToken);
}