using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionAcknowledgeMessage :  IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionAcknowledgeMessage;
    
    public required long ConsumerId { get; init; }
    public required long MessageId { get; init; }
    public required bool RequiresResponse { get; init; }
    
    public int GetRequiredBufferSize()
    {
        var byteCount = 0;
        
        byteCount += sizeof(long); // ConsumerId
        byteCount += sizeof(long); // MessageId
        byteCount += sizeof(bool); // RequiresResponse
        
        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), ConsumerId);
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), MessageId);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), RequiresResponse);
        
        return offset;
    }
}