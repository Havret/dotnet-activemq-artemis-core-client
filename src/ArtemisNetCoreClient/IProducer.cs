namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// A producer sends messages to ActiveMQ Artemis addresses.
/// </summary>
public interface IProducer : IAsyncDisposable
{
    /// <summary>
    /// Sends a message synchronously to the broker. This method is primarily used for non-durable
    /// message delivery since it does not wait for a confirmation from the broker and operates
    /// in a fire-and-forget manner.
    /// </summary>
    /// <remarks>
    /// This method should typically be used when message delivery speed is prioritized over reliability.
    /// The message will be sent with 'at most once' delivery guarantee, as there's no acknowledgment
    /// from the broker that the message has been received or persisted.
    /// </remarks> 
    void SendMessage(Message message);
    
    /// <summary>
    /// Sends a message asynchronously to the broker. This method supports both durable and non-durable message
    /// delivery modes, as specified by the message's durable property. It awaits for a confirmation
    /// from the broker, ensuring that the message is either stored (for durable messages) or acknowledged
    /// (for non-durable messages) before completing.
    /// </summary>
    /// <remarks>
    /// This method should be used when reliability is required, and it supports awaiting the acknowledgment
    /// from the broker. The delivery semantics are 'at least once' for durable messages, where the broker
    /// confirms the persistence of the message. For non-durable messages, the completion of the task
    /// indicates that the broker has received the message.
    /// </remarks> 
    Task SendMessageAsync(Message message, CancellationToken cancellationToken = default);
}