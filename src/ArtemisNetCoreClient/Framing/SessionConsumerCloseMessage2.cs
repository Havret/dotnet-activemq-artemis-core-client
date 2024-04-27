namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionConsumerCloseMessage2 : Packet
{
    public const byte Type = 74;
    
    public long ConsumerId { get; set; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteLong(ConsumerId);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

internal readonly struct SessionConsumerCloseMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionConsumerCloseMessage;
    public required long ConsumerId { get; init; }
    public int GetRequiredBufferSize()
    {
        return sizeof(long);
    }

    public int Encode(Span<byte> buffer)
    {
        return ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), ConsumerId);
    }
}