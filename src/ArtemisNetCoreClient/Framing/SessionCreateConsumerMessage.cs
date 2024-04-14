namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionCreateConsumerMessage : Packet
{
    public const byte Type = 40;

    public required long Id { get; init; }
    public required string QueueName { get; init; }
    public string? FilterString { get; init; }
    public required int Priority { get; init; }
    public required bool BrowseOnly { get; init; }
    public required bool RequiresResponse { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteLong(Id);
        buffer.WriteByteString(QueueName);
        buffer.WriteNullableByteString(FilterString);
        buffer.WriteBool(BrowseOnly);
        buffer.WriteBool(RequiresResponse);
        buffer.WriteInt(Priority);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}