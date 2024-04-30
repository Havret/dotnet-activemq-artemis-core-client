namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class ActiveMQExceptionMessage : Packet
{
    public const byte Type = 20;

    public int Code { get; private set; }
    public string? Message { get; set; }
    
    public virtual void Encode(ByteBuffer buffer)
    {
    }

    public virtual void Decode(ByteBuffer buffer)
    {
        Code = buffer.ReadInt();
        Message = buffer.ReadNullableString();
    }
}