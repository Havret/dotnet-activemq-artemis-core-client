namespace ActiveMQ.Artemis.Core.Client;

public interface IConsumer : IAsyncDisposable
{
    ValueTask<ReceivedMessage> ReceiveMessageAsync(CancellationToken cancellationToken);
}