namespace ActiveMQ.Artemis.Core.Client.Framing;

internal abstract class Packet
{
    public virtual long CorrelationId => -1;
    public virtual bool IsResponse => false;

    public virtual void Encode(ByteBuffer buffer)
    {
        
    }

    public virtual void Decode(ByteBuffer buffer)
    {
        
    }
}