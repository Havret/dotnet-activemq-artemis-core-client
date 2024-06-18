namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Provides configuration options for creating sessions in an ActiveMQ Artemis message producer.
/// </summary>
public class ProducerConfiguration
{
    /// <summary>
    /// Gets or sets the name of the address to send messages to.
    /// </summary>
    public required string Address { get; init; }
    
    /// <summary>
    /// Gets or sets the routing type to use when sending messages.
    /// </summary>
    public RoutingType? RoutingType { get; init; }
}