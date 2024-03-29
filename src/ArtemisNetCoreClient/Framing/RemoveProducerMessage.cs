namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class RemoveProducerMessage : Packet
{
    public const byte Type = unchecked((byte) -21);

    public required int Id { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteInt(Id);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}