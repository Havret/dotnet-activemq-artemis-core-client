namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionQueueQueryMessage : Packet
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