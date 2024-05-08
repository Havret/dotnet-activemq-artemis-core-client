namespace ActiveMQ.Artemis.Core.Client;

public readonly struct ReadOnlyHeaders
{
    public required long MessageId { get; init; }
    
    public required string Address { get; init; }
    
    public required Guid? UserId { get; init; }
    
    public required byte Type { get; init; }
    
    public required bool Durable { get; init; }
    
    // GMT milliseconds at which this message expires. 0 means never expires *
    public required  long Expiration { get; init; }
    
    // TODO: Change to DateTime
    public required long Timestamp { get; init; }
    
    // TODO: Enum?
    public required byte Priority { get; init; }
}