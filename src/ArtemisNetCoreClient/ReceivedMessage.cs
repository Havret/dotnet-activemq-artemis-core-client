namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// <see cref="ReceivedMessage"/> is used to receive data from ActiveMQ Artemis queues.
/// When sending messages, <see cref="Message"/> should be used.
/// </summary>
public class ReceivedMessage
{
    /// <summary>
    /// The message headers
    /// </summary>
    public required ReadOnlyHeaders Headers { get; init; }
    
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
}