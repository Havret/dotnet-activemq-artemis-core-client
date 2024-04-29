using System.Runtime.CompilerServices;
using ActiveMQ.Artemis.Core.Client.Utils;

namespace ActiveMQ.Artemis.Core.Client.Framing;

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
        byteCount += sizeof(byte); // Properties size nullability
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
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetReference(), Message.Body.Length + Header.HeaderSize + BodyOffset);
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
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), Message.Headers.Expiration);
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), Message.Headers.Timestamp);
        offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetOffset(offset), Message.Headers.Priority);
        
        return offset;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EncodeProperties(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetReference(), DataConstants.Null);
        
        return offset;
    }
}