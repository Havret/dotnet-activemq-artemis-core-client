namespace ActiveMQ.Artemis.Core.Client;

public interface IConnection : IAsyncDisposable
{
    Task<ISession> CreateSession(CancellationToken cancellationToken = default);
}