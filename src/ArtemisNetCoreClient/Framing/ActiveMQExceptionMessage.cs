using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct ActiveMQExceptionMessage : IIncomingPacket
{
    public readonly int Code;
    public readonly string? Message;
    public readonly long CorrelationId;
    
    public ActiveMQExceptionMessage(ReadOnlySpan<byte> buffer)
    {
        var readBytes = 0;
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer, out Code);
        readBytes += ArtemisBinaryConverter.ReadNullableString(buffer[readBytes..], out Message);
        if (buffer.Length - readBytes >= sizeof(long))
        {
            readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out CorrelationId);
        }
        
        Debug.Assert(readBytes == buffer.Length, $"Expected to read {buffer.Length} bytes but got {readBytes}");
    }
}