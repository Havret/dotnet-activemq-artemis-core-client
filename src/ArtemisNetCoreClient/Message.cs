namespace ActiveMQ.Artemis.Core.Client;

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