namespace ActiveMQ.Artemis.Core.Client.Framing;

internal abstract class Packet
{
    public long CorrelationId { get; set; } = -1;
    public virtual bool IsResponse => false;

    public abstract void Encode(ByteBuffer buffer);

    public abstract void Decode(ByteBuffer buffer);
}