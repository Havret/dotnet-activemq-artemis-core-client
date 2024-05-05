using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

public readonly struct SessionQueueQueryResponseMessage : IIncomingPacket
{
    public readonly bool Exists;
    public readonly bool Durable;
    public readonly bool Temporary;
    public readonly int ConsumerCount;
    public readonly long MessageCount;
    public readonly string? FilterString;
    public readonly string? Address;
    public readonly string? Name;
    public readonly bool AutoCreateQueues;
    public readonly bool AutoCreated;
    public readonly bool PurgeOnNoConsumers;
    public readonly RoutingType RoutingType;
    public readonly int MaxConsumers;
    public readonly bool? Exclusive;
    public readonly bool? LastValue;
    public readonly int? DefaultConsumerWindowSize;
    public readonly string? LastValueKey;
    public readonly bool? NonDestructive;
    public readonly int? ConsumersBeforeDispatch;
    public readonly long? DelayBeforeDispatch;
    public readonly bool? GroupRebalance;
    public readonly int? GroupBuckets;
    public readonly bool? AutoDelete;
    public readonly long? AutoDeleteDelay;
    public readonly long? AutoDeleteMessageCount;
    public readonly string? GroupFirstKey;
    public readonly long? RingSize;
    public readonly bool? Enabled;
    public readonly bool? GroupRebalancePauseDispatch;
    public readonly bool? ConfigurationManaged;

    public SessionQueueQueryResponseMessage(ReadOnlySpan<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.ReadBool(buffer, out Exists);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out Durable);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out Temporary);
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out ConsumerCount);
        offset += ArtemisBinaryConverter.ReadInt64(buffer[offset..], out MessageCount);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out FilterString);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out Address);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out Name);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out AutoCreateQueues);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out AutoCreated);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out PurgeOnNoConsumers);
        offset += ArtemisBinaryConverter.ReadByte(buffer[offset..], out var routingType);
        RoutingType = (RoutingType) routingType;
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out MaxConsumers);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out Exclusive);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out LastValue);
        offset += ArtemisBinaryConverter.ReadNullableInt32(buffer[offset..], out DefaultConsumerWindowSize);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out LastValueKey);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out NonDestructive);
        offset += ArtemisBinaryConverter.ReadNullableInt32(buffer[offset..], out ConsumersBeforeDispatch);
        offset += ArtemisBinaryConverter.ReadNullableInt64(buffer[offset..], out DelayBeforeDispatch);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out GroupRebalance);
        offset += ArtemisBinaryConverter.ReadNullableInt32(buffer[offset..], out GroupBuckets);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out AutoDelete);
        offset += ArtemisBinaryConverter.ReadNullableInt64(buffer[offset..], out AutoDeleteDelay);
        offset += ArtemisBinaryConverter.ReadNullableInt64(buffer[offset..], out AutoDeleteMessageCount);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out GroupFirstKey);
        offset += ArtemisBinaryConverter.ReadNullableInt64(buffer[offset..], out RingSize);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out Enabled);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out GroupRebalancePauseDispatch);
        offset += ArtemisBinaryConverter.ReadNullableBool(buffer[offset..], out ConfigurationManaged);
        
        Debug.Assert(offset == buffer.Length, "offset == buffer.Length");
    }
}