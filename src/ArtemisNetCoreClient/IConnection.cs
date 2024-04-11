namespace ActiveMQ.Artemis.Core.Client;

public interface IConnection : IAsyncDisposable
{
    Task<ISession> CreateSession(Endpoint endpoint, CancellationToken cancellationToken = default);
}