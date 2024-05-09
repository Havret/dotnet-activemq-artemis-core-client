namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Contains the state associated with a message delivery.
/// </summary>
public readonly struct MessageDelivery
{
    internal MessageDelivery(long consumerId, long messageId)
    {
        ConsumerId = consumerId;
        MessageId = messageId;
    }
    
    public long ConsumerId { get; }
    public long MessageId { get; }
}