using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct CreateQueueMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.CreateQueueMessage;
    
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

    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(Address);
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(QueueName);
        byteCount += ArtemisBinaryConverter.GetNullableStringByteCount(FilterString);
        byteCount += sizeof(bool); // Durable
        byteCount += sizeof(bool); // Temporary
        byteCount += sizeof(bool); // RequiresResponse
        byteCount += sizeof(bool); // AutoCreated
        byteCount += sizeof(byte); // RoutingType
        byteCount += sizeof(int); // MaxConsumers
        byteCount += sizeof(bool); // PurgeOnNoConsumers
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(Exclusive);
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(LastValue);
        byteCount += ArtemisBinaryConverter.GetNullableSimpleStringByteCount(LastValueKey);
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(NonDestructive);
        byteCount += ArtemisBinaryConverter.GetNullableInt32ByteCount(ConsumersBeforeDispatch);
        byteCount += ArtemisBinaryConverter.GetNullableInt64ByteCount(DelayBeforeDispatch);
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(GroupRebalance);
        byteCount += ArtemisBinaryConverter.GetNullableInt32ByteCount(GroupBuckets);
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(AutoDelete);
        byteCount += ArtemisBinaryConverter.GetNullableInt64ByteCount(AutoDeleteDelay);
        byteCount += ArtemisBinaryConverter.GetNullableInt64ByteCount(AutoDeleteMessageCount);
        byteCount += ArtemisBinaryConverter.GetNullableSimpleStringByteCount(GroupFirstKey);
        byteCount += ArtemisBinaryConverter.GetNullableInt64ByteCount(RingSize);
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(Enabled);
        byteCount += ArtemisBinaryConverter.GetNullableBoolByteCount(GroupRebalancePauseDispatch);
        
        return byteCount;

    }
    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetReference(), Address);
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetOffset(offset), QueueName);
        offset += ArtemisBinaryConverter.WriteNullableString(ref buffer.GetOffset(offset), FilterString);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), Durable);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), Temporary);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), RequiresResponse);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), AutoCreated);
        offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetOffset(offset), (byte) RoutingType);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), MaxConsumers);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), PurgeOnNoConsumers);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), Exclusive);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), LastValue);
        offset += ArtemisBinaryConverter.WriteNullableSimpleString(ref buffer.GetOffset(offset), LastValueKey);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), NonDestructive);
        offset += ArtemisBinaryConverter.WriteNullableInt32(ref buffer.GetOffset(offset), ConsumersBeforeDispatch);
        offset += ArtemisBinaryConverter.WriteNullableInt64(ref buffer.GetOffset(offset), DelayBeforeDispatch);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), GroupRebalance);
        offset += ArtemisBinaryConverter.WriteNullableInt32(ref buffer.GetOffset(offset), GroupBuckets);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), AutoDelete);
        offset += ArtemisBinaryConverter.WriteNullableInt64(ref buffer.GetOffset(offset), AutoDeleteDelay);
        offset += ArtemisBinaryConverter.WriteNullableInt64(ref buffer.GetOffset(offset), AutoDeleteMessageCount);
        offset += ArtemisBinaryConverter.WriteNullableSimpleString(ref buffer.GetOffset(offset), GroupFirstKey);
        offset += ArtemisBinaryConverter.WriteNullableInt64(ref buffer.GetOffset(offset), RingSize);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), Enabled);
        offset += ArtemisBinaryConverter.WriteNullableBool(ref buffer.GetOffset(offset), GroupRebalancePauseDispatch);
        return offset;
    }
}