namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class CreateQueueMessageV2 : Packet
{
    public const byte Type = unchecked((byte) -12);
    
    public required string Address { get; init; }
    public required string QueueName { get; init; }
    public string? FilterString { get; init; }
    public bool Durable { get; init; }
    public bool Temporary { get; init; }
    public bool RequiresResponse { get; init; }
    public bool AutoCreated { get; init; }
    public required RoutingType RoutingType { get; init; }
    public int MaxConsumers { get; init; }
    public bool PurgeOnNoConsumers { get; init; }
    public bool? Exclusive { get; init; }
    public bool? LastValue { get; init; }
    public string? LastValueKey { get; init; }
    public bool? NonDestructive { get; init; }
    public int? ConsumersBeforeDispatch { get; init; }
    public long? DelayBeforeDispatch { get; init; }
    public bool? GroupRebalance { get; init; }
    public int? GroupBuckets { get; init; }
    public bool? AutoDelete { get; init; }
    public long? AutoDeleteDelay { get; init; }
    public long? AutoDeleteMessageCount { get; init; }
    public string? GroupFirstKey { get; init; }
    public long? RingSize { get; init; }
    public bool? Enabled { get; init; }
    public bool? GroupRebalancePauseDispatch { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteAmqString(Address);
        buffer.WriteAmqString(QueueName);
        buffer.WriteNullableString(FilterString);
        buffer.WriteBool(Durable);
        buffer.WriteBool(Temporary);
        buffer.WriteBool(RequiresResponse);
        buffer.WriteBool(AutoCreated);
        buffer.WriteByte((byte) RoutingType);
        buffer.WriteInt(MaxConsumers);
        buffer.WriteBool(PurgeOnNoConsumers);
        buffer.WriteNullableBool(Exclusive);
        buffer.WriteNullableBool(LastValue);
        buffer.WriteNullableAmqString(LastValueKey);
        buffer.WriteNullableBool(NonDestructive);
        buffer.WriteNullableInt(ConsumersBeforeDispatch);
        buffer.WriteNullableLong(DelayBeforeDispatch);
        buffer.WriteNullableBool(GroupRebalance);
        buffer.WriteNullableInt(GroupBuckets);
        buffer.WriteNullableBool(AutoDelete);
        buffer.WriteNullableLong(AutoDeleteDelay);
        buffer.WriteNullableLong(AutoDeleteMessageCount);
        buffer.WriteNullableAmqString(GroupFirstKey);
        buffer.WriteNullableLong(RingSize);
        buffer.WriteNullableBool(Enabled);
        buffer.WriteNullableBool(GroupRebalancePauseDispatch);
    }

    public override void Decode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }
}