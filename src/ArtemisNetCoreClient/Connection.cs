using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Connection : IConnection, IChannel
{
    private readonly ILogger<Connection> _logger;
    private readonly Transport2 _transport;
    private readonly Task _receiveLoopTask;
    private readonly Dictionary<long, IChannel> _channels = new();

    public Connection(ILoggerFactory loggerFactory, Transport2 transport, Endpoint endpoint)
    {
        _logger = loggerFactory.CreateLogger<Connection>();
        _transport = transport;
        _channels.Add(1, this);

        _receiveLoopTask = Task.Run(ReceiveLoop);
    }
    
    private void ReceiveLoop()
    {
        // TODO: Handle loop exit
        while (true)
        {
            var inboundPacket = _transport.ReceivePacket();
            if (_channels.TryGetValue(inboundPacket.ChannelId, out var channel))
            {
                channel.OnPacket(ref inboundPacket);
            }
            else
            {
                _logger.LogWarning("Received packet for unknown channel {ChannelId}", inboundPacket.ChannelId);
            }
        }
    }

    public Task<ISession> CreateSession(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public void OnPacket(ref readonly InboundPacket packet)
    {
        throw new NotImplementedException();
    }
}

internal interface IChannel
{
    void OnPacket(ref readonly InboundPacket packet);
}