using System.Net.Sockets;
using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

internal class Transport(Socket socket) : IAsyncDisposable
{
    public async Task SendAsync(Packet packet, CancellationToken cancellationToken)
    {
        var byteBuffer = new ByteBuffer();
        Codec.Encode(byteBuffer, packet, 1);
        await socket.SendAsync(byteBuffer.GetBuffer(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<Packet> ReceiveAsync(CancellationToken cancellationToken)
    {
        var receiveBuffer = new byte[sizeof(int)];
        while (0 == await socket.ReceiveAsync(receiveBuffer, cancellationToken).ConfigureAwait(false))
        {
        }
        
        var size = new ByteBuffer(receiveBuffer).ReadInt();
        
        var buffer = new byte[size];
        _ = await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

        var payloadBuffer = new ByteBuffer(buffer);

        var (packet, _) = Codec.Decode(payloadBuffer);
        return packet;
    }

    public ValueTask DisposeAsync()
    {
        socket.Dispose();
        return ValueTask.CompletedTask; 
    }
}