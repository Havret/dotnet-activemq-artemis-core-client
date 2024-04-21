namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionStop : Packet
{
    public const byte Type = 68;
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

internal readonly struct SessionStop2 : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionStop;

    public int GetRequiredBufferSize() => 0;

    public int Encode(Span<byte> buffer) => 0;
}