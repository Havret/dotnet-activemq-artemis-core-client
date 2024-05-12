using System.Runtime.CompilerServices;
using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionSendMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionSendMessage;
    
    public required Message Message { get; init; }
    public required bool RequiresResponse { get; init; }
    public required long CorrelationId { get; init; }
    public required int ProducerId { get; init; }

    public int GetRequiredBufferSize()
    {
        int byteCount = 0;

        byteCount += sizeof(int); // Message body offset
        byteCount += sizeof(int); // Message body length
        byteCount += Message.Body.Length; // Actual message body length
        byteCount += sizeof(long); // MessageId
        byteCount += ArtemisBinaryConverter.GetNullableSimpleStringByteCount(Message.Headers.Address);
        byteCount += ArtemisBinaryConverter.GetNullableGuidByteCount(Message.Headers.UserId);
        byteCount += sizeof(byte); // Type
        byteCount += sizeof(bool); // Durable
        byteCount += sizeof(long); // Expiration
        byteCount += sizeof(long); // Timestamp
        byteCount += sizeof(byte); // Priority
        
        byteCount += sizeof(byte); // Properties nullability
        if (Message.Properties?.Count > 0)
        {
            byteCount += sizeof(int); // Properties count
            foreach (var (key, value) in Message.Properties)
            {
                byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(key);
                byteCount += ArtemisBinaryConverter.GetNullableObjectByteCount(value);
            }
        }
        
        byteCount += sizeof(bool); // RequiresResponse
        byteCount += sizeof(long); // CorrelationId
        byteCount += sizeof(int); // ProducerId

        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;

        offset += EncodeMessageBody(buffer);
        offset += EncodeHeaders(buffer[offset..]);
        offset += EncodeProperties(buffer[offset..]);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), RequiresResponse);
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), CorrelationId);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), ProducerId);

        return offset;
    }

    private const int BodyOffset = sizeof(int);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EncodeMessageBody(Span<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetReference(), Message.Body.Length + PacketHeader.HeaderSize + BodyOffset);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Message.Body.Length);

        if (Message.Body.Length > 0)
        {
            Message.Body.Span.CopyTo(buffer[offset..]);
            offset += Message.Body.Length;
        }

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EncodeHeaders(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), Message.Headers.MessageId);
        offset += ArtemisBinaryConverter.WriteNullableSimpleString(ref buffer.GetOffset(offset), Message.Headers.Address);
        offset += ArtemisBinaryConverter.WriteNullableGuid(ref buffer.GetOffset(offset), Message.Headers.UserId);
        offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetOffset(offset), Message.Headers.Type);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), Message.Headers.Durable);
        
        offset += EncodeExpiration(buffer[offset..]);
        
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), Message.Headers.Timestamp);
        offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetOffset(offset), Message.Headers.Priority);
        
        return offset;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EncodeExpiration(Span<byte> buffer)
    {
        var offset = 0;

        var expiration = Message.Headers.Expiration != DateTimeOffset.MinValue
            ? Message.Headers.Expiration.ToUnixTimeMilliseconds()
            : 0;
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetReference(), expiration);
        
        return offset;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EncodeProperties(Span<byte> buffer)
    {
        var offset = 0;

        if (Message.Properties?.Count > 0)
        {
            offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetReference(), DataConstants.NotNull);
            offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Message.Properties.Count);
            foreach (var (key, value) in Message.Properties)
            {
                offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetOffset(offset), key);
                offset += ArtemisBinaryConverter.WriteNullableObject(ref buffer.GetOffset(offset), value);
            }
        }
        else
        {
            offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetReference(), DataConstants.Null);
        }

        return offset;
    }
}