namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct InboundPacket
{
    public long ChannelId { get; init; }
    public PacketType PacketType { get; init; }
    public ArraySegment<byte> Payload { get; init; }
}