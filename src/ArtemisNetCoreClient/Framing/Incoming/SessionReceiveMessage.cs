using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

internal readonly struct SessionReceiveMessage : IIncomingPacket
{
    public readonly ReceivedMessage Message;
    public readonly long ConsumerId;
    public readonly int DeliveryCount;

    public SessionReceiveMessage(ReadOnlySpan<byte> buffer)
    {
        var readBytes = 0;
        readBytes += DecodeMessageBody(buffer, out var body);
        readBytes += DecodeHeaders(buffer[readBytes..], out var headers);
        readBytes += DecodeProperties(buffer[readBytes..], out var properties);
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out ConsumerId);
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out DeliveryCount);
        
        Message = new ReceivedMessage
        {
            Body = body,
            Headers = headers,
            Properties = properties,
            MessageDelivery = new MessageDelivery(ConsumerId, headers.MessageId)
        };
        
        Debug.Assert(readBytes == buffer.Length, $"Expected to read {buffer.Length} bytes but got {readBytes}");
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
    private static int DecodeHeaders(ReadOnlySpan<byte> buffer, out ReadOnlyHeaders value)
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

        value = new ReadOnlyHeaders
        {
            MessageId = messageId,
            Address = address ?? "",
            UserId = userId,
            Type = type,
            Durable = durable,
            Expiration = expiration == 0 ? DateTimeOffset.MinValue : DateTimeOffset.FromUnixTimeMilliseconds(expiration),
            Timestamp = timestamp,
            Priority = priority
        };

        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeProperties(ReadOnlySpan<byte> buffer, out IReadOnlyDictionary<string, object?> value)
    {
        var readBytes = ArtemisBinaryConverter.ReadByte(buffer, out var isNotNull);
        if (isNotNull == DataConstants.NotNull)
        {
            readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out var count);
            var properties = new Dictionary<string, object?>(count);
            for (var i = 0; i < count; i++)
            {
                readBytes += ArtemisBinaryConverter.ReadSimpleString(buffer[readBytes..], out var key);
                readBytes += ArtemisBinaryConverter.ReadNullableObject(buffer[readBytes..], out var obj);
                properties.Add(key, obj);
            }

            value = properties;
            
            return readBytes;
        }
        else
        {
            value = ReadOnlyDictionary<string, object?>.Empty;
            return readBytes;
        }
    }
}