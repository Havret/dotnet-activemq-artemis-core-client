namespace ActiveMQ.Artemis.Core.Client;

public interface IConsumer : IAsyncDisposable
{
    ValueTask<ReceivedMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Acknowledges all messages received by the consumer so far.
    /// </summary>
    ValueTask AcknowledgeAsync(MessageDelivery messageDelivery, CancellationToken cancellationToken);
    
    /// <summary>
    /// Acknowledges the message.
    /// </summary>
    /// <param name="messageDelivery">Delivery information of a message to acknowledge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask IndividualAcknowledgeAsync(in MessageDelivery messageDelivery, CancellationToken cancellationToken);
    
    /// <summary>
    /// Acknowledges all messages received by the consumer. It doesn't wait for the confirmation from the broker.
    /// It's a fire-and-forget operation. If you need to wait for the confirmation, use <see cref="AcknowledgeAsync"/> instead.
    /// </summary>
    /// <param name="messageDelivery">Delivery information of a last message to acknowledge.</param>
    void Acknowledge(in MessageDelivery messageDelivery);
    
    /// <summary>
    /// Acknowledges the message without waiting for the confirmation from the broker.
    /// It's a fire-and-forget operation. If you need to wait for the confirmation, use <see cref="IndividualAcknowledgeAsync"/> instead.
    /// </summary>
    /// <param name="messageDelivery">Delivery information of a message to acknowledge.</param>
    void IndividualAcknowledge(in MessageDelivery messageDelivery);
}