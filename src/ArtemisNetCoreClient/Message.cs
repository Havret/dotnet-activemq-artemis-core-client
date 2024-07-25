namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// <see cref="Message"/> is used to send data to ActiveMQ Artemis addresses. When receiving messages,
/// the message is returned as a <see cref="ReceivedMessage"/>.
/// </summary>
public class Message
{
    public long MessageId { get; set; }
    
    public Guid? UserId { get; set; }
    
    public byte Type { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the message is durable or not.
    /// Durable messages are persisted in permanent storage and will survive server failure or restart.
    /// Non-durable messages will not survive server failure or restart.
    /// </summary>
    public bool Durable { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this message expires. If set to <see cref="DateTimeOffset.MinValue"/>,
    /// the message is considered to never expire.
    /// Note that when setting this property, any time precision finer than milliseconds will be lost.
    /// </summary>
    public DateTimeOffset Expiration { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when this message was created.
    /// Note that when setting this property, any time precision finer than milliseconds will be lost.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
    
    // TODO: Enum?
    public byte Priority { get; set; }

    /// <summary>
    /// The message properties
    /// </summary>
    public IDictionary<string, object?>? Properties { get; set; }
    
    /// <summary>
    /// The message body (payload)
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; set; }

    /// <summary>
    /// The Group ID used when sending the message. This is used for message grouping feature. Messages with the same message group
    /// are always consumed by the same consumer, even if multiple consumers are listening on the same queue. 
    /// </summary>
    public string? GroupId { get; set; }
}