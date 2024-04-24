namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct SessionBindingQueryMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionBindingQueryMessage;
    public required string Address { get; init; }
    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(Address);
        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetReference(), Address);
        return offset;
    }
}

internal class SessionBindingQueryMessage2 : Packet
{
    public const byte Type = 49;
    
    public required string Address { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteByteString(Address);
    }

    public override void Decode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }
}