namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionConsumerFlowCreditMessage : Packet
{
    public const byte Type = 70;
    
    public long ConsumerId { get; init; }
    public int Credits { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteLong(ConsumerId);
        buffer.WriteInt(Credits);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}