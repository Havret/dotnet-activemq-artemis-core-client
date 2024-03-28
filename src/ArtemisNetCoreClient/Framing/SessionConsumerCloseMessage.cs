namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionConsumerCloseMessage : Packet
{
    public const byte Type = 74;
    
    public long ConsumerId { get; set; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteLong(ConsumerId);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}