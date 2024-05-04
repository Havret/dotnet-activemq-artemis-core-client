using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing;

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