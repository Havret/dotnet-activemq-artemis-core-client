using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

internal readonly struct SessionQueueQueryResponseMessage : IIncomingPacket
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
        var readBytes = 0;
        readBytes += ArtemisBinaryConverter.ReadBool(buffer, out Exists);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out Durable);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out Temporary);
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out ConsumerCount);
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out MessageCount);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out FilterString);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out Address);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out Name);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out AutoCreateQueues);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out AutoCreated);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out PurgeOnNoConsumers);
        readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var routingType);
        RoutingType = (RoutingType) routingType;
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out MaxConsumers);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out Exclusive);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out LastValue);
        readBytes += ArtemisBinaryConverter.ReadNullableInt32(buffer[readBytes..], out DefaultConsumerWindowSize);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out LastValueKey);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out NonDestructive);
        readBytes += ArtemisBinaryConverter.ReadNullableInt32(buffer[readBytes..], out ConsumersBeforeDispatch);
        readBytes += ArtemisBinaryConverter.ReadNullableInt64(buffer[readBytes..], out DelayBeforeDispatch);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out GroupRebalance);
        readBytes += ArtemisBinaryConverter.ReadNullableInt32(buffer[readBytes..], out GroupBuckets);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out AutoDelete);
        readBytes += ArtemisBinaryConverter.ReadNullableInt64(buffer[readBytes..], out AutoDeleteDelay);
        readBytes += ArtemisBinaryConverter.ReadNullableInt64(buffer[readBytes..], out AutoDeleteMessageCount);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out GroupFirstKey);
        readBytes += ArtemisBinaryConverter.ReadNullableInt64(buffer[readBytes..], out RingSize);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out Enabled);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out GroupRebalancePauseDispatch);
        readBytes += ArtemisBinaryConverter.ReadNullableBool(buffer[readBytes..], out ConfigurationManaged);
        
        Debug.Assert(readBytes == buffer.Length, $"Expected to read {buffer.Length} bytes but got {readBytes}");
    }
}