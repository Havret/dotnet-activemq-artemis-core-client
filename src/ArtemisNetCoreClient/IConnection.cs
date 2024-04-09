namespace ActiveMQ.Artemis.Core.Client;

public interface IConnection
{
    Task<ISession> CreateSession(Endpoint endpoint, CancellationToken cancellationToken = default);
}