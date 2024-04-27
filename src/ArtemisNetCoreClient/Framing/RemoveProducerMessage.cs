using ActiveMQ.Artemis.Core.Client.Utils;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct RemoveProducerMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.RemoveProducerMessage;
    
    public required int Id { get; init; }
    
    public int GetRequiredBufferSize()
    {
        return sizeof(int);
    }

    public int Encode(Span<byte> buffer)
    {
        return ArtemisBinaryConverter.WriteInt32(ref buffer.GetReference(), Id);
    }
}