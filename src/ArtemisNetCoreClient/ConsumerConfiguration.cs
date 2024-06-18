namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Provides configuration options for creating sessions in an ActiveMQ Artemis message consumer.
/// </summary>
public class ConsumerConfiguration
{
    /// <summary>
    /// Gets or sets the name of the queue to consume messages from.
    /// </summary>
    public required string QueueName { get; init; }
}