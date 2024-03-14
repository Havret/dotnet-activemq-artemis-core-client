namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class NullResponse : Packet
{
    public const byte Type = 68;
    
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}