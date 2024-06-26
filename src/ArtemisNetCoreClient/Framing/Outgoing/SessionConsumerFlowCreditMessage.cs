using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionConsumerFlowCreditMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionConsumerFlowCreditMessage;
    
    public required long ConsumerId { get; init; }
    public required int Credits { get; init; }

    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        
        byteCount += sizeof(long); // ConsumerId
        byteCount += sizeof(int); // Credits
        
        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), ConsumerId);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Credits);

        return offset;
    }
}