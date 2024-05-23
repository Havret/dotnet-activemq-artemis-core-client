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
}