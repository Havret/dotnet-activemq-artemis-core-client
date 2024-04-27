namespace ActiveMQ.Artemis.Core.Client;

internal interface IOutgoingPacket
{
    PacketType PacketType { get; }
    int GetRequiredBufferSize();
    int Encode(Span<byte> buffer);
}