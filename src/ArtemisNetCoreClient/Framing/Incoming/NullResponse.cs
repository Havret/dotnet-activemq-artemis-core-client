using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

internal readonly struct NullResponse : IIncomingPacket
{
    public readonly long CorrelationId;
    
    public NullResponse(ReadOnlySpan<byte> buffer)
    {
        var readBytes = 0;
        if (buffer.IsEmpty)
        {
            CorrelationId = -1;
        }
        else
        {
            readBytes += ArtemisBinaryConverter.ReadInt64(buffer, out CorrelationId);
        }

        Debug.Assert(readBytes == buffer.Length, $"Expected to read {buffer.Length} bytes but got {readBytes}");
    }
}