namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionCloseMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionCloseMessage;

    public int GetRequiredBufferSize() => 0;

    public int Encode(Span<byte> buffer) => 0;
}