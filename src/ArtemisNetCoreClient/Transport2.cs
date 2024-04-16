using System.Buffers;
using System.Net.Sockets;
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
                    MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment);
                    ArrayPool<byte>.Shared.Return(segment.Array!);
                }
            }

            await _writer.FlushAsync();
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
    
    internal InboundPacket ReceivePacket()
    {
        Span<byte> frameSizeBuffer = stackalloc byte[sizeof(int)];
        _ = _reader.Read(frameSizeBuffer);
        _ = ArtemisBinaryConverter.ReadInt32(frameSizeBuffer, out var frameSize);
        
        Span<byte> typeBuffer = stackalloc byte[sizeof(byte)];
        _ = _reader.Read(typeBuffer);
        _ = ArtemisBinaryConverter.ReadByte(typeBuffer, out var packetType);
        
        Span<byte> channelIdBuffer = stackalloc byte[sizeof(long)];
        _ = _reader.Read(channelIdBuffer);
        var channelId = ArtemisBinaryConverter.ReadInt64(typeBuffer);

        var payloadBufferSize = frameSize - sizeof(byte) - sizeof(long);

        var buffer = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
        
        _ = _reader.Read(buffer.AsSpan(0, payloadBufferSize));

        return new InboundPacket
        {
            PacketType = (PacketType) packetType,
            ChannelId = channelId,
            Payload = new ArraySegment<byte>(buffer, 0, payloadBufferSize)
        };
    }
}

internal readonly ref struct InboundPacket
{
    public long ChannelId { get; init; }
    public PacketType PacketType { get; init; }
    public ArraySegment<byte> Payload { get; init; }
}

internal enum PacketType : byte
{
    CreateSessionMessage = unchecked((byte) -18),
    CreateSessionResponse = 31
}