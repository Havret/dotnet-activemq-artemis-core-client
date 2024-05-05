using System.Buffers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;

namespace ActiveMQ.Artemis.Core.Client;

internal class Transport : IAsyncDisposable
{
    private readonly ILogger<Transport> _logger;
    private readonly Socket _socket;
    private readonly ChannelReader<ReadOnlyMemory<byte>> _channelReader;
    private readonly ChannelWriter<ReadOnlyMemory<byte>> _channelWriter;
    private readonly Task _sendLoopTask;
    private readonly BufferedStream _reader;
    private readonly BufferedStream _writer;

    public Transport(ILogger<Transport> logger, Socket socket)
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

    private async ValueTask<PacketHeader> ReadHeaderAsync(CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(PacketHeader.HeaderSize);
        try
        {
            await _reader.ReadExactlyAsync(buffer, 0, HeaderSize, cancellationToken);
            return new PacketHeader(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}