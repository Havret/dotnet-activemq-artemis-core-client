namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// This enum represents the routing types that dictate how messages are handled by an address in the Apache ActiveMQ Artemis.
/// </summary>
public enum RoutingType : byte
{
    /// <summary>
    /// 'Multicast' routing type: Used when a message should be delivered to multiple queues.
    /// Each bound queue at the address receives a copy of the message. This is ideal for
    /// scenarios where the same message needs to be processed by different consumers
    /// independently, akin to a publish-subscribe model.
    /// </summary>
    Multicast = 0,

    /// <summary>
    /// 'Anycast' routing type: Used when a message should be delivered to one of the many
    /// queues bound to an address, typically chosen in a round-robin fashion. This routing
    /// type is suitable for point-to-point communication where a message is intended for
    /// only one consumer among potentially many. It ensures that each message is delivered
    /// to exactly one of the available queues attached to the address.
    /// </summary>
    Anycast = 1
}