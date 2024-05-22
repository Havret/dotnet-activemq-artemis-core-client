namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Represents a message producer capable of sending messages to multiple addresses.
/// </summary>
public interface IAnonymousProducer : IAsyncDisposable
{
    /// <summary>
    /// Sends a message to the specified address.
    /// </summary>
    /// <param name="address">The address to which the message should be sent.</param>
    /// <param name="routingType">
    /// The routing type to use when sending the message. Ensures that this message is only routed to queues with matching routing type.
    /// </param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask SendMessageAsync(string address, RoutingType? routingType, Message message, CancellationToken cancellationToken = default);
}