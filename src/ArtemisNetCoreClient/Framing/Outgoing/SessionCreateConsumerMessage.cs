using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionCreateConsumerMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionCreateConsumerMessage;
    
    public required long Id { get; init; }
    public required string QueueName { get; init; }
    public required string? FilterString { get; init; }
    public required bool BrowseOnly { get; init; }
    public required bool RequiresResponse { get; init; }
    public required int Priority { get; init; }

    
    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        
        byteCount += sizeof(long); // Id
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(QueueName);
        byteCount += ArtemisBinaryConverter.GetNullableSimpleStringByteCount(FilterString);
        byteCount += sizeof(bool); // BrowseOnly
        byteCount += sizeof(bool); // RequiresResponse
        byteCount += sizeof(int); // Priority

        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), Id);
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetOffset(offset), QueueName);
        offset += ArtemisBinaryConverter.WriteNullableSimpleString(ref buffer.GetOffset(offset), FilterString);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), BrowseOnly);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), RequiresResponse);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Priority);

        return offset;
    }
}