namespace ActiveMQ.Artemis.Core.Client;

public interface IConnection : IAsyncDisposable
{
    Task<ISession> CreateSessionAsync(SessionConfiguration configuration, CancellationToken cancellationToken = default);
}