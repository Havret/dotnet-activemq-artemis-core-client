namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionQueueQueryMessage2 : Packet
{
    public const byte Type = 45;

    public required string QueueName { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteByteString(QueueName);
    }

    public override void Decode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }
}

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