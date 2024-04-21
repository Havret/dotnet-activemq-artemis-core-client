namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionCloseMessage : Packet
{
    public const byte Type = 69;
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

internal readonly struct SessionCloseMessage2 : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionCloseMessage;

    public int GetRequiredBufferSize() => 0;

    public int Encode(Span<byte> buffer) => 0;
}