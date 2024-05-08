namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// <see cref="Message"/> is used to send data to ActiveMQ Artemis addresses. When receiving messages,
/// the message is returned as a <see cref="ReceivedMessage"/>.
/// </summary>
public class Message
{
    /// <summary>
    /// The message headers
    /// </summary>
    public Headers Headers { get; set; }
    
    /// <summary>
    /// The message properties
    /// </summary>
    public IDictionary<string, object?> Properties { get; set; }
    
    /// <summary>
    /// The message body (payload)
    /// </summary>
    public ReadOnlyMemory<byte> Body { get; set; }
}