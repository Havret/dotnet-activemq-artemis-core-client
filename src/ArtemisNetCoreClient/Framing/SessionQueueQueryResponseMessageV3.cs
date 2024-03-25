namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionQueueQueryResponseMessageV3 : Packet
{
    public const byte Type = unchecked((byte) -14);

    public override bool IsResponse => true;

    public bool Exists { get; private set; }
    public bool Durable { get; private set; }
    public bool Temporary { get; private set; }
    public int ConsumerCount { get; private set; }
    public long MessageCount { get; private set; }
    public string? FilterString { get; private set; }
    public string? Address { get; private set; }
    public string? Name { get; private set; }
    public bool AutoCreateQueues { get; private set; }
    public bool AutoCreated { get; private set; }
    public bool PurgeOnNoConsumers { get; private set; }
    public RoutingType RoutingType { get; private set; }
    public int MaxConsumers { get; private set; }
    public bool? Exclusive { get; private set; }
    public bool? LastValue { get; private set; }
    public int? DefaultConsumerWindowSize { get; private set; }
    public string? LastValueKey { get; private set; }
    public bool? NonDestructive { get; private set; }
    public int? ConsumersBeforeDispatch { get; private set; }
    public long? DelayBeforeDispatch { get; private set; }
    public bool? GroupRebalance { get; private set; }
    public int? GroupBuckets { get; private set; }
    public bool? AutoDelete { get; private set; }
    public long? AutoDeleteDelay { get; private set; }
    public long? AutoDeleteMessageCount { get; private set; }
    public string? GroupFirstKey { get; private set; }
    public long? RingSize { get; private set; }
    public bool? Enabled { get; private set; }
    public bool? GroupRebalancePauseDispatch { get; private set; }
    public bool? ConfigurationManaged { get; private set; }
    

    public override void Encode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }

    public override void Decode(ByteBuffer buffer)
    {
        Exists = buffer.ReadBool();
        Durable = buffer.ReadBool();
        Temporary = buffer.ReadBool();
        ConsumerCount = buffer.ReadInt();
        MessageCount = buffer.ReadLong();
        FilterString = buffer.ReadNullableByteString();
        Address = buffer.ReadNullableByteString();
        Name = buffer.ReadNullableByteString();
        AutoCreateQueues = buffer.ReadBool();
        AutoCreated = buffer.ReadBool();
        PurgeOnNoConsumers = buffer.ReadBool();
        RoutingType = (RoutingType) buffer.ReadByte();
        MaxConsumers =  buffer.ReadInt();
        Exclusive =  buffer.ReadNullableBool();
        LastValue =  buffer.ReadNullableBool();
        DefaultConsumerWindowSize =  buffer.ReadNullableInt();
        LastValueKey =  buffer.ReadNullableByteString();
        NonDestructive =  buffer.ReadNullableBool();
        ConsumersBeforeDispatch =  buffer.ReadNullableInt();
        DelayBeforeDispatch =  buffer.ReadNullableLong();
        GroupRebalance =  buffer.ReadNullableBool();
        GroupBuckets =  buffer.ReadNullableInt();
        AutoDelete =  buffer.ReadNullableBool();
        AutoDeleteDelay =  buffer.ReadNullableLong();
        AutoDeleteMessageCount =  buffer.ReadNullableLong();
        GroupFirstKey =  buffer.ReadNullableByteString();
        RingSize =  buffer.ReadNullableLong();
        Enabled =  buffer.ReadNullableBool();
        GroupRebalancePauseDispatch =  buffer.ReadNullableBool();
        ConfigurationManaged =  buffer.ReadNullableBool();
    }
}