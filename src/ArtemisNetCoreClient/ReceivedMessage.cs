namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// <see cref="ReceivedMessage"/> is used to receive data from ActiveMQ Artemis queues. When sending messages,
/// the <see cref="Message"/> should be used.
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
    public ReadOnlyMemory<byte> Body { get; init; }
}