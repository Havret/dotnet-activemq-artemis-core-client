namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionCloseMessage : Packet
{
    public const byte Type = 69;
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}