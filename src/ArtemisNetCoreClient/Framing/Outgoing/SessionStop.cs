namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionStop : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionStop;

    public int GetRequiredBufferSize() => 0;

    public int Encode(Span<byte> buffer) => 0;
}