namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Provides extension methods for the <see cref="IAnonymousProducer"/> interface.
/// </summary>
public static class AnonymousProducerExtensions
{
    /// <summary>
    /// Sends a message to the specified address.
    /// </summary>
    /// <param name="anonymousProducer">The <see cref="IAnonymousProducer"/> instance that this method extends.</param>
    /// <param name="address">The address to which the message should be sent.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static ValueTask SendMessageAsync(this IAnonymousProducer anonymousProducer, string address, Message message, CancellationToken cancellationToken = default)
    {
        return anonymousProducer.SendMessageAsync(address, routingType: null, message, cancellationToken);
    }
}