namespace ActiveMQ.Artemis.Core.Client.Framing;

internal interface IOutgoingPacket
{
    PacketType PacketType { get; }
    int GetRequiredBufferSize();
    int Encode(Span<byte> buffer);
}