using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Connection : IConnection
{
    private readonly ILogger<Connection> _logger;
    private readonly Transport2 _transport;
    private readonly Task _receiveLoopTask;

    public Connection(ILoggerFactory loggerFactory, Transport2 transport)
    {
        _logger = loggerFactory.CreateLogger<Connection>();
        _transport = transport;
        
        

        _receiveLoopTask = Task.Run(ReceiveLoop);
    }
    
    private Task ReceiveLoop()
    {
        while (true)
        {
            var inboundFrame = _transport.ReceivePacket();
        }
        
        return Task.CompletedTask;
    }

    public Task<ISession> CreateSession(Endpoint endpoint, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}