using System.Buffers;
using System.Collections.Concurrent;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Connection : IConnection, IChannel
{
    private readonly ILogger<Connection> _logger;
    private readonly Transport2 _transport;
    private readonly Endpoint _endpoint;
    private readonly Task _receiveLoopTask;
    private readonly Dictionary<long, IChannel> _channels = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource<IIncomingPacket>> _completionSources = new();

    public Connection(ILoggerFactory loggerFactory, Transport2 transport, Endpoint endpoint)
    {
        _logger = loggerFactory.CreateLogger<Connection>();
        _transport = transport;
        _endpoint = endpoint;
        _channels.Add(1, this);

        _receiveLoopTask = Task.Run(ReceiveLoop);
    }
    
    private void ReceiveLoop()
    {
        // TODO: Handle loop exit
        while (true)
        {
            var inboundPacket = _transport.ReceivePacket();
            try
            {
                if (_channels.TryGetValue(inboundPacket.ChannelId, out var channel))
                {
                    channel.OnPacket(ref inboundPacket);
                }
                else
                {
                    _logger.LogWarning("Received packet for unknown channel {ChannelId}", inboundPacket.ChannelId);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(inboundPacket.Payload.Array!);
            }
        }
    }
    
    public void OnPacket(ref readonly InboundPacket packet)
    {
        switch (packet.PacketType)
        {
            case PacketType.CreateSessionResponse:
                
                break;
            default:
                _logger.LogWarning("Received unexpected packet type {PacketType}", packet.PacketType);
                break;
        }
    }

    public Task<ISession> CreateSession(CancellationToken cancellationToken = default)
    {
        var createSessionMessage = new CreateSessionMessage
        {
            Name = Guid.NewGuid().ToString(),
            SessionChannelId = 10,
            Version = 135,
            Username = _endpoint.User,
            Password = _endpoint.Password,
            MinLargeMessageSize = 100 * 1024,
            Xa = false,
            AutoCommitSends = true,
            AutoCommitAcks = true,
            PreAcknowledge = false,
            WindowSize = -1,
            DefaultAddress = null,
            ClientId = null,
        };
        

        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}

internal interface IChannel
{
    void OnPacket(ref readonly InboundPacket packet);
}

internal interface IOutgoingPacket
{
    PacketType PacketType { get; }
    int GetRequiredBufferSize();
    void Encode(Span<byte> buffer);
}

internal interface IIncomingPacket
{

    void Decode(ReadOnlySpan<byte> buffer);
}