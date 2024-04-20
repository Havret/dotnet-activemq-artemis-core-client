namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionStart : Packet
{
    public const byte Type = 67;
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

internal readonly struct SessionStart2 : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionStart;

    public int GetRequiredBufferSize() => 0;

    public int Encode(Span<byte> buffer) => 0;
}