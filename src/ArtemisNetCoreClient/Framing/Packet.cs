namespace ActiveMQ.Artemis.Core.Client.Framing;

internal abstract class Packet
{
    public abstract byte Type { get;  }
    public abstract void Encode(ByteBuffer buffer);
    public abstract void Decode(ByteBuffer buffer);
}