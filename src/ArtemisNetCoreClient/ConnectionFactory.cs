using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ActiveMQ.Artemis.Core.Client;

public class ConnectionFactory
{
    public async Task<IConnection> CreateAsync(Endpoint endpoint, CancellationToken cancellationToken = default)
    {
        return new Connection(LoggerFactory);
    }
    
    public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();
}