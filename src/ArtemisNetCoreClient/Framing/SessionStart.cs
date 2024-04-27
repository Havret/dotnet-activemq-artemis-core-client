namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct SessionStart : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionStart;

    public int GetRequiredBufferSize() => 0;

    public int Encode(Span<byte> buffer) => 0;
}