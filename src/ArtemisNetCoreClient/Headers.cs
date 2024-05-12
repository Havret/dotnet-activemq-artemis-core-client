namespace ActiveMQ.Artemis.Core.Client;

public struct Headers
{
    public long MessageId { get; set; }
    
    public string? Address { get; set; }
    
    public Guid? UserId { get; set; }
    
    public byte Type { get; set; }
    
    public bool Durable { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this message expires. If set to <see cref="DateTimeOffset.MinValue"/>,
    /// the message is considered to never expire.
    /// Note that when setting this property, any time precision finer than milliseconds will be lost.
    /// </summary>
    public DateTimeOffset Expiration { get; set; }
    
    // TODO: Change to DateTime
    public long Timestamp { get; set; }
    
    // TODO: Enum?
    public byte Priority { get; set; }
}