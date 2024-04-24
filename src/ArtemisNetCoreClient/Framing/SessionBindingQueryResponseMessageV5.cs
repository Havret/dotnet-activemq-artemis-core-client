using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct SessionBindingQueryResponseMessage : IIncomingPacket
{
    public readonly bool Exists;
    public readonly string[] QueueNames;
    public readonly bool AutoCreateQueues;
    public readonly bool AutoCreateAddresses;
    public readonly bool DefaultPurgeOnNoConsumers;
    public readonly int DefaultMaxConsumers;
    public readonly bool? DefaultExclusive;
    public readonly bool? DefaultLastValue;
    public readonly string? DefaultLastValueKey;
    public readonly bool? DefaultNonDestructive;
    public readonly int? DefaultConsumersBeforeDispatch;
    public readonly long? DefaultDelayBeforeDispatch;
    public readonly bool SupportsMulticast;
    public readonly bool SupportsAnycast;
    
    public SessionBindingQueryResponseMessage(ReadOnlySpan<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.ReadBool(buffer, out Exists);
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out var numQueues);
        QueueNames = new string[numQueues];
        for (var i = 0; i < numQueues; i++)
        {
            offset += ArtemisBinaryConverter.ReadSimpleString(buffer[offset..], out QueueNames[i]);
        }
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out AutoCreateQueues);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out AutoCreateAddresses);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out DefaultPurgeOnNoConsumers);
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out DefaultMaxConsumers);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out DefaultExclusive);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out DefaultLastValue);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out DefaultLastValueKey);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out DefaultNonDestructive);
        offset += ArtemisBinaryConverter.ReadNullableInt32(buffer[offset..], out DefaultConsumersBeforeDispatch);
        offset += ArtemisBinaryConverter.ReadNullableInt64(buffer[offset..], out DefaultDelayBeforeDispatch);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out SupportsMulticast);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out SupportsAnycast);
        
        Debug.Assert(offset == buffer.Length, "offset == buffer.Length");
    }
}

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