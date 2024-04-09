using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Connection : IConnection
{
    public Connection(ILoggerFactory loggerFactory)
    {
    }

    public Task<ISession> CreateSession(Endpoint endpoint, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}