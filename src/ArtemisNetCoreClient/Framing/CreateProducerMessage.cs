using ActiveMQ.Artemis.Core.Client.Utils;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct CreateProducerMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.CreateProducerMessage;
    
    public required int Id { get; init; }
    public required string? Address { get; init; }
    
    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        
        byteCount += sizeof(int); // Id
        byteCount += ArtemisBinaryConverter.GetNullableSimpleStringByteCount(Address);

        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Id);
        offset += ArtemisBinaryConverter.WriteNullableSimpleString(ref buffer.GetOffset(offset), Address);

        return offset;
    }
}