namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionSendMessageV3 : Packet
{
    public const byte Type = 71;

    public required Message Message { get; init; }
    public bool RequiresResponse { get; init; }
    public int ProducerId { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        Message.Encode(buffer);
        buffer.WriteBool(RequiresResponse);
        buffer.WriteLong(CorrelationId);
        buffer.WriteInt(ProducerId);
    }

    public override void Decode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }
}