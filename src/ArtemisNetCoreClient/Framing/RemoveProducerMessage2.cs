namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class RemoveProducerMessage2 : Packet
{
    public const byte Type = unchecked((byte) -21);

    public required int Id { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteInt(Id);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

internal readonly struct RemoveProducerMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.RemoveProducerMessage;
    
    public required int Id { get; init; }
    
    public int GetRequiredBufferSize()
    {
        return sizeof(int);
    }

    public int Encode(Span<byte> buffer)
    {
        return ArtemisBinaryConverter.WriteInt32(ref buffer.GetReference(), Id);
    }
}