namespace ActiveMQ.Artemis.Core.Client.Framing;

public struct Headers
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
}