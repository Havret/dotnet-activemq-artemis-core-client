namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class CreateProducerMessage2 : Packet
{
    public const byte Type = unchecked((byte) -20);

    public required int Id { get; init; }
    public string? Address { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteInt(Id);
        buffer.WriteNullableByteString(Address);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

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