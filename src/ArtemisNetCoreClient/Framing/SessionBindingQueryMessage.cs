namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionBindingQueryMessage : Packet
{
    public const byte Type = 49;
    
    public required string Address { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteStringAsBytes(Address);
    }

    public override void Decode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }
}