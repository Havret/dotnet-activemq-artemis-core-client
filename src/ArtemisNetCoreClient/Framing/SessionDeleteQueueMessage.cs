using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct SessionDeleteQueueMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionDeleteQueueMessage;

    public required string QueueName { get; init; }

    public int GetRequiredBufferSize()
    {
        var byteCount = 0;
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(QueueName);
        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetReference(), QueueName);
        return offset;
    }
}