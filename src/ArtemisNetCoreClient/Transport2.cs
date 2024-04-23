using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Transport2 : IAsyncDisposable
{
    private readonly ILogger<Transport2> _logger;
    private readonly Socket _socket;
    private readonly ChannelReader<ReadOnlyMemory<byte>> _channelReader;
    private readonly ChannelWriter<ReadOnlyMemory<byte>> _channelWriter;
    private readonly Task _sendLoopTask;
    private readonly BufferedStream _reader;
    private readonly BufferedStream _writer;

    public Transport2(ILogger<Transport2> logger, Socket socket)
    {
        _logger = logger;
        _socket = socket;
        
        var channel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = false,
            SingleReader = true,
            SingleWriter = false
        });
        
        _channelReader = channel.Reader;
        _channelWriter = channel.Writer;

        var networkStream = new NetworkStream(socket);
        
        _reader = new BufferedStream(networkStream, _socket.ReceiveBufferSize);
        _writer = new BufferedStream(networkStream, _socket.SendBufferSize);
        
        _sendLoopTask = Task.Run(SendLoop);
    }
    
    public void Send(ReadOnlyMemory<byte> memory)
    {
        _channelWriter.TryWrite(memory);
    }

    private async Task SendLoop()
    {
        try
        {
            while (await _channelReader.WaitToReadAsync())
            {
                while (_channelReader.TryRead(out var memory))
                {
                    await _writer.WriteAsync(memory);
                    MemoryMarshal.TryGetArray(memory, out var segment);
                    ArrayPool<byte>.Shared.Return(segment.Array!);
                }
                await _writer.FlushAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Background socket write loop has crashed");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channelWriter.Complete();
        await _sendLoopTask;
        _socket.Dispose();
    }
    
    internal async ValueTask<InboundPacket> ReceivePacketAsync(CancellationToken cancellationToken)
    {
        var header = await ReadHeaderAsync(cancellationToken);
        var payloadSize = header.FrameSize - sizeof(byte) - sizeof(long);

        var buffer = ArrayPool<byte>.Shared.Rent(payloadSize);
        try
        {
            await _reader.ReadExactlyAsync(buffer, 0, payloadSize, cancellationToken);
            return new InboundPacket
            {
                PacketType = header.PacketType,
                ChannelId = header.ChannelId,
                Payload = new ArraySegment<byte>(buffer, 0, payloadSize)
            };
        }
        catch (Exception)
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw;
        }
    }

    private const int HeaderSize = sizeof(int) + sizeof(byte) + sizeof(long);
    private async ValueTask<Header> ReadHeaderAsync(CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Header.HeaderSize);
        try
        {
            await _reader.ReadExactlyAsync(buffer, 0, HeaderSize, cancellationToken);
            return new Header(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

internal readonly struct Header
{
    public const int HeaderSize = sizeof(int) + sizeof(byte) + sizeof(long);

    public Header(ReadOnlySpan<byte> buffer)
    {
        var readBytes = ArtemisBinaryConverter.ReadInt32(buffer, out FrameSize);
        readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var packetType);
        PacketType = (PacketType) packetType;
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out ChannelId);
        
        Debug.Assert(readBytes == HeaderSize, $"Expected to read {HeaderSize} bytes but got {readBytes}");
    }

    public readonly int FrameSize;
    public readonly long ChannelId;
    public readonly PacketType PacketType;
}

internal readonly struct InboundPacket
{
    public long ChannelId { get; init; }
    public PacketType PacketType { get; init; }
    public ArraySegment<byte> Payload { get; init; }
}

internal enum PacketType : byte
{
    CreateSessionMessage = unchecked((byte) -18),
    CreateSessionResponse = 31,
    SessionStart = 67,
    NullResponse = 21,
    SessionStop = 68,
    SessionCloseMessage = 69,
}