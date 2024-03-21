namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class NullResponse : Packet
{
    public const byte Type = 21;
    public override bool IsResponse => true;
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}