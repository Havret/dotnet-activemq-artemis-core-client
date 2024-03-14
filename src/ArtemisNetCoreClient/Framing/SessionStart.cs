namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionStart : Packet
{
    public const byte Type = 67;
    
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}