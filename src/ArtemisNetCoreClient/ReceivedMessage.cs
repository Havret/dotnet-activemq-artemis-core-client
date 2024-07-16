namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// <see cref="ReceivedMessage"/> is used to receive data from ActiveMQ Artemis queues.
/// When sending messages, <see cref="Message"/> should be used.
/// </summary>
public class ReceivedMessage
{
    public required long MessageId { get; init; }
    
    public required string Address { get; init; }
    
    public required Guid? UserId { get; init; }
    
    public required byte Type { get; init; }
    
    /// <summary>
    /// Gets a value indicating whether the message is durable or not.
    /// Durable messages are persisted in permanent storage and will survive server failure or restart.
    /// Non-durable messages will not survive server failure or restart.
    /// </summary>
    public required bool Durable { get; init; }
    
    /// <summary>
    /// Gets the date and time when this message expires. If set to <see cref="DateTimeOffset.MinValue"/>,
    /// the message is considered to never expire.
    /// </summary>
    public DateTimeOffset Expiration { get; init; }

    /// <summary>
    /// Gets the date and time when this message was created.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }
    
    // TODO: Enum?
    public required byte Priority { get; init; }
    
    /// <summary>
    /// The message properties
    /// </summary>
    public required IReadOnlyDictionary<string, object?> Properties { get; init; }
    
    /// <summary>
    /// The message body (payload)
    /// </summary>
    public required ReadOnlyMemory<byte> Body { get; init; }
    
    /// <summary>
    /// Gets the information about the message delivery. It can be used to acknowledge the message later
    /// even if the message has been disposed or discarded.
    /// </summary>
    public required MessageDelivery MessageDelivery { get; init; }
    
    /// <summary>
    /// The routing type used when sending the message.
    /// </summary>
    public required RoutingType? RoutingType { get; init; }
}