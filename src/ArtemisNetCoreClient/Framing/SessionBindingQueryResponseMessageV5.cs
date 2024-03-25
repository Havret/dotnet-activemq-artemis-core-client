namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionBindingQueryResponseMessageV5 : Packet
{
    public const byte Type = unchecked((byte) -22);

    public override bool IsResponse => true;

    public bool Exists { get; set; }
    public IReadOnlyList<string> QueueNames { get; set; }
    public bool AutoCreateQueues { get; set; }
    public bool AutoCreateAddresses { get; set; }
    public bool DefaultPurgeOnNoConsumers { get; set; }
    public int DefaultMaxConsumers { get; set; }
    public bool? DefaultExclusive { get; set; }
    public bool? DefaultLastValue { get; set; }
    public string? DefaultLastValueKey { get; set; }
    public bool? DefaultNonDestructive { get; set; }
    public int? DefaultConsumersBeforeDispatch { get; set; }
    public long? DefaultDelayBeforeDispatch { get; set; }
    public bool SupportsMulticast { get; set; }
    public bool SupportsAnycast { get; set; }

    public override void Encode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }

    public override void Decode(ByteBuffer buffer)
    {
        Exists = buffer.ReadBool();
        var numQueues = buffer.ReadInt();
        var queueNames = new string[numQueues];
        for (int i = 0; i < numQueues; i++)
        {
            queueNames[i] = buffer.ReadByteString();
        }

        QueueNames = queueNames;
        AutoCreateQueues = buffer.ReadBool();
        AutoCreateAddresses = buffer.ReadBool();
        DefaultPurgeOnNoConsumers = buffer.ReadBool();
        DefaultMaxConsumers = buffer.ReadInt();
        DefaultExclusive = buffer.ReadNullableBool();
        DefaultLastValue = buffer.ReadNullableBool();
        DefaultLastValueKey = buffer.ReadNullableByteString();
        DefaultNonDestructive = buffer.ReadNullableBool();
        DefaultConsumersBeforeDispatch = buffer.ReadNullableInt();
        DefaultDelayBeforeDispatch = buffer.ReadNullableLong();
        SupportsMulticast = buffer.ReadBool();
        SupportsAnycast = buffer.ReadBool();
    }
}