namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class CreateProducerMessage : Packet
{
    public const byte Type = unchecked((byte) -20);

    public required int Id { get; init; }
    public string? Address { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteInt(Id);
        buffer.WriteNullableByteString(Address);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}