using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct SessionQueueQueryMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionQueueQueryMessage;
    
    public required string QueueName { get; init; }
    
    public int GetRequiredBufferSize()
    {
        return ArtemisBinaryConverter.GetSimpleStringByteCount(QueueName);
    }

    public int Encode(Span<byte> buffer)
    {
        return ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetReference(), QueueName);
    }
}