using System.Net.Sockets;

namespace ActiveMQ.Artemis.Core.Client;

public interface ISession : IAsyncDisposable;

internal class Session(Socket socket) : ISession
{
    public ValueTask DisposeAsync()
    {
        socket.Dispose();
        return ValueTask.CompletedTask;
    }
}