namespace ActiveMQ.Artemis.Core.Client;

public interface IConnection : IAsyncDisposable
{
    Task<ISession> CreateSessionAsync(CancellationToken cancellationToken = default);
}