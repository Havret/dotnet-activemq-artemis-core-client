namespace ActiveMQ.Artemis.Core.Client.Framing;

internal abstract class Packet
{
    public virtual long CorrelationId => -1;
    public virtual bool IsResponse => false;

    public abstract void Encode(ByteBuffer buffer);

    public abstract void Decode(ByteBuffer buffer);
}