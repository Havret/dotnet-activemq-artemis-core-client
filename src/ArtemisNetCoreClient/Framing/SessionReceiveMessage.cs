namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionReceiveMessage : Packet
{
    public const byte Type = 75;

    public Message Message { get; private set; }
    public long ConsumerId { get; private set; }
    public int DeliveryCount { get; private set; }
    
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
        Message = new Message();
        Message.Decode(buffer);
        ConsumerId = buffer.ReadLong();
        DeliveryCount = buffer.ReadInt();
    }
}