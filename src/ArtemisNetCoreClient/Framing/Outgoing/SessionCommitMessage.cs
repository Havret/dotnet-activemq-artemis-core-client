using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionCommitMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionCommitMessage;
    
    public required long CorrelationId { get; init; }
    
    public int GetRequiredBufferSize()
    {
        var byteCount = 0;
        
        byteCount += sizeof(long); // CorrelationId
        
        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), CorrelationId);
        
        return offset;
    }
}