namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// A consumer receives messages from ActiveMQ Artemis queues.
/// </summary>
public interface IConsumer : IAsyncDisposable
{
    /// <summary>
    /// Receives a message from the broker. This operation will asynchronously wait for a message to be available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask<ReceivedMessage> ReceiveMessageAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Acknowledges all messages received by the consumer so far and asynchronously awaits for the confirmation from the broker.
    /// </summary>
    /// <param name="messageDelivery">Delivery information of a last message to acknowledge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask AcknowledgeAsync(MessageDelivery messageDelivery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges the message and asynchronously awaits for the confirmation from the broker.
    /// </summary>
    /// <param name="messageDelivery">Delivery information of a message to acknowledge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask IndividualAcknowledgeAsync(in MessageDelivery messageDelivery, CancellationToken cancellationToken = default);
    
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