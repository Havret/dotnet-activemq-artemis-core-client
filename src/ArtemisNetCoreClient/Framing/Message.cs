namespace ActiveMQ.Artemis.Core.Client.Framing;

public class Message
{
    public long MessageId { get; set; }
    public string? Address { get; set; }
    public Guid? UserId { get; set; }
    public byte Type { get; set; }
    public bool Durable { get; set; }
    
    // GMT milliseconds at which this message expires. 0 means never expires *
    public long Expiration { get; set; }
    
    // TODO: Change to DateTime
    public long Timestamp { get; set; }
    
    // TODO: Enum?
    public byte Priority { get; set; }
    
    public byte[] Body { get; set; }
    
    internal void Encode(ByteBuffer buffer)
    {
        // TODO: Add comment why it's 17 ;-)
        buffer.WriteInt(Body.Length + 17);
        buffer.WriteInt(Body.Length);
        buffer.WriteBytes(Body);
        
        buffer.WriteLong(MessageId);
        buffer.WriteNullableByteString(Address);

        if (UserId != null)
        {
            buffer.WriteByte(DataConstants.NotNull);
            // TODO: Encode UserID
        }
        else
        {
            buffer.WriteByte(DataConstants.Null);
        }
        
        buffer.WriteByte(Type);
        buffer.WriteBool(Durable);
        buffer.WriteLong(Expiration);
        buffer.WriteLong(Timestamp);
        buffer.WriteByte(Priority);
        
        // TODO: Encode Properties Metadata
        buffer.WriteByte(DataConstants.Null);
    }

    internal void Decode(ByteBuffer buffer)
    {
        // Drop first
        _ = buffer.ReadInt();
        var bodyLength = buffer.ReadInt();
        Body = new byte[bodyLength];
        buffer.ReadBytes(Body);

        MessageId = buffer.ReadLong();
        Address = buffer.ReadNullableByteString();

        if (buffer.ReadByte() == DataConstants.NotNull)
        {
            // TODO: Dencode UserID
        }

        Type = buffer.ReadByte();

        Durable = buffer.ReadBool();
        Expiration = buffer.ReadLong();
        Timestamp = buffer.ReadLong();
        Priority = buffer.ReadByte();
    }
}