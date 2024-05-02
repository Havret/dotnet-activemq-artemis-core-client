using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct SessionReceiveMessage : IIncomingPacket
{
    public readonly Message Message;
    public readonly long ConsumerId;
    public readonly int DeliveryCount;

    public SessionReceiveMessage(ReadOnlySpan<byte> buffer)
    {
        var offset = 0;
        offset += DecodeMessageBody(buffer, out var body);
        offset += DecodeHeaders(buffer[offset..], out var headers);
        offset += DecodeProperties(buffer[offset..], out var properties);
        Message = new Message
        {
            Body = body,
            Headers = headers,
            Properties = properties
        };
        offset += ArtemisBinaryConverter.ReadInt64(buffer[offset..], out ConsumerId);
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out DeliveryCount);
        
        Debug.Assert(offset == buffer.Length, $"Expected to read {buffer.Length} bytes but got {offset}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeMessageBody(ReadOnlySpan<byte> buffer, out ReadOnlyMemory<byte> value)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.ReadInt32(buffer, out _); // Skip body offset;
        offset += ArtemisBinaryConverter.ReadInt32(buffer[offset..], out var bodyLength);

        byte[] body = new byte[bodyLength];
        buffer.Slice(offset, bodyLength).CopyTo(body);
        value = new ReadOnlyMemory<byte>(body);
        
        return offset + bodyLength;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeHeaders(ReadOnlySpan<byte> buffer, out Headers value)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.ReadInt64(buffer, out var messageId);
        offset += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[offset..], out var address);
        offset += ArtemisBinaryConverter.ReadNullableGuid(buffer[offset..], out var userId);
        offset += ArtemisBinaryConverter.ReadByte(buffer[offset..], out var type);
        offset += ArtemisBinaryConverter.ReadBool(buffer[offset..], out var durable);
        offset += ArtemisBinaryConverter.ReadInt64(buffer[offset..], out var expiration);
        offset += ArtemisBinaryConverter.ReadInt64(buffer[offset..], out var timestamp);
        offset += ArtemisBinaryConverter.ReadByte(buffer[offset..], out var priority);

        value = new Headers
        {
            MessageId = messageId,
            Address = address,
            UserId = userId,
            Type = type,
            Durable = durable,
            Expiration = expiration,
            Timestamp = timestamp,
            Priority = priority
        };

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeProperties(ReadOnlySpan<byte> buffer, out IDictionary<string, object?> value)
    {
        var readBytes = ArtemisBinaryConverter.ReadByte(buffer, out var isNotNull);
        if (isNotNull == DataConstants.NotNull)
        {
            readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out var count);
            value = new Dictionary<string, object?>(count);
            for (var i = 0; i < count; i++)
            {
                readBytes += ArtemisBinaryConverter.ReadSimpleString(buffer[readBytes..], out var key);
                readBytes += ArtemisBinaryConverter.ReadNullableObject(buffer[readBytes..], out var obj);
                value.Add(key, obj);
            }
            
            return readBytes;
        }
        else
        {
            value = ReadOnlyDictionary<string, object?>.Empty;
            return readBytes;
        }
    }
}