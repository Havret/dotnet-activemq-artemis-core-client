namespace ActiveMQ.Artemis.Core.Client;

public interface ISession : IAsyncDisposable;

internal class Session(Transport socket) : ISession
{
    public async ValueTask DisposeAsync()
    {
        await socket.DisposeAsync().ConfigureAwait(false);
    }
}