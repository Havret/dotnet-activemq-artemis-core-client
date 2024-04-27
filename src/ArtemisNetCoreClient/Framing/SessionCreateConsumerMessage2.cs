namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class SessionCreateConsumerMessage2 : Packet
{
    public const byte Type = 40;

    public required long Id { get; init; }
    public required string QueueName { get; init; }
    public string? FilterString { get; init; }
    public required int Priority { get; init; }
    public required bool BrowseOnly { get; init; }
    public required bool RequiresResponse { get; init; }
    
    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteLong(Id);
        buffer.WriteByteString(QueueName);
        buffer.WriteNullableByteString(FilterString);
        buffer.WriteBool(BrowseOnly);
        buffer.WriteBool(RequiresResponse);
        buffer.WriteInt(Priority);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}

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
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), Id);
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetOffset(offset), QueueName);
        offset += ArtemisBinaryConverter.WriteNullableSimpleString(ref buffer.GetOffset(offset), FilterString);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), BrowseOnly);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), RequiresResponse);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Priority);

        return offset;
    }
}