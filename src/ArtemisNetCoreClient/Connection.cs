using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Connection : IConnection, IChannel
{
    private readonly ILogger<Connection> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Transport2 _transport;
    private readonly Endpoint _endpoint;
    private readonly Task _receiveLoopTask;
    private readonly ConcurrentDictionary<long, IChannel> _channels = new();
    private readonly ConcurrentDictionary<long, TaskCompletionSource<IIncomingPacket>> _completionSources = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IdGenerator _sessionChannelIdGenerator = new(10);
    private readonly CancellationTokenSource _receiveLoopCancellationToken;

    public Connection(ILoggerFactory loggerFactory, Transport2 transport, Endpoint endpoint)
    {
        _logger = loggerFactory.CreateLogger<Connection>();
        _loggerFactory = loggerFactory;
        _transport = transport;
        _endpoint = endpoint;
        _channels.TryAdd(1, this);

        _receiveLoopCancellationToken = new CancellationTokenSource();
        _receiveLoopTask = Task.Run(ReceiveLoop);
    }
    
    private async Task ReceiveLoop()
    {
        while (_receiveLoopCancellationToken.IsCancellationRequested == false)
        {
            try
            {
                var inboundPacket = await _transport.ReceivePacketAsync(_receiveLoopCancellationToken.Token);
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
                    if (inboundPacket.Payload.Array is { } array)
                    {
                        ArrayPool<byte>.Shared.Return(array);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (IOException e)
            {
                _logger.LogError(e, "Error in network communication");
            }
        }
    }
    
    public void OnPacket(ref readonly InboundPacket packet)
    {
        switch (packet.PacketType)
        {
            case PacketType.CreateSessionResponse:
                var createSessionResponseMessage = new CreateSessionResponseMessage2(packet.Payload);
                if (_completionSources.TryRemove(-1, out var tcs))
                {
                    tcs.TrySetResult(createSessionResponseMessage);
                }
                break;
            default:
                _logger.LogWarning("Received unexpected packet type {PacketType}", packet.PacketType);
                break;
        }
    }

    public async Task<ISession> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        var createSessionMessage = new CreateSessionMessage
        {
            Name = Guid.NewGuid().ToString(),
            SessionChannelId = _sessionChannelIdGenerator.GenerateId(),
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

        try
        {
            await _lock.WaitAsync(cancellationToken);
            var tcs = new TaskCompletionSource<IIncomingPacket>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = _completionSources.TryAdd(-1, tcs);
            Send(ref createSessionMessage, 1);
            var incomingPacket = (CreateSessionResponseMessage2) await tcs.Task;

            var session = new Session(this, _loggerFactory)
            {
                ServerVersion = incomingPacket.ServerVersion,
                ChannelId = createSessionMessage.SessionChannelId
            };
            
            _channels.TryAdd(session.ChannelId, session);
            
            session.StartAsync2();

            return session;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    internal void Send<T>(ref readonly T packet, long channelId) where T : IOutgoingPacket
    {
        const int headerSize = sizeof(int) + sizeof(byte) + sizeof(long);
        var size = headerSize + packet.GetRequiredBufferSize();
        var buffer = ArrayPool<byte>.Shared.Rent(size);
        
        var offset = ArtemisBinaryConverter.WriteInt32(ref buffer.AsSpan().GetReference(), size - sizeof(int));
        offset += ArtemisBinaryConverter.WriteByte(ref buffer.AsSpan().GetOffset(offset), (byte) packet.PacketType);
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.AsSpan().GetOffset(offset), channelId);
        offset += packet.Encode(buffer.AsSpan(offset));
        
        Debug.Assert(size == offset, $"Size mismatch, expected {size} but got {offset}");
        
        _transport.Send(buffer.AsMemory(0, size));
    }
    
    internal void RemoveChannel(long channelId)
    {
        _channels.TryRemove(channelId, out _);
    }

    public async ValueTask DisposeAsync()
    {
        await _receiveLoopCancellationToken.CancelAsync();
        _receiveLoopCancellationToken.Dispose();
        await _transport.DisposeAsync();
        await _receiveLoopTask;
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
    int Encode(Span<byte> buffer);
}

internal interface IIncomingPacket;

