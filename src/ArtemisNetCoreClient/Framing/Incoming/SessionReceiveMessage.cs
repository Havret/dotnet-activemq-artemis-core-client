using System.Collections.Frozen;
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
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out var messageId);
        readBytes += ArtemisBinaryConverter.ReadNullableSimpleString(buffer[readBytes..], out var address);
        readBytes += ArtemisBinaryConverter.ReadNullableGuid(buffer[readBytes..], out var userId);
        readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var type);
        readBytes += ArtemisBinaryConverter.ReadBool(buffer[readBytes..], out var durable);
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out var expiration);
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out var timestamp);
        readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var priority);
        readBytes += DecodeProperties(buffer[readBytes..], out var properties, out var routingType, out var groupId);
        readBytes += ArtemisBinaryConverter.ReadInt64(buffer[readBytes..], out ConsumerId);
        readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out DeliveryCount);
        
        Message = new ReceivedMessage
        {
            Body = body,
            MessageId = messageId,
            Address = address ?? "",
            UserId = userId,
            Type = type,
            Durable = durable,
            Expiration = expiration == 0 ? DateTimeOffset.MinValue : DateTimeOffset.FromUnixTimeMilliseconds(expiration),
            Timestamp =  timestamp == 0 ? DateTimeOffset.MinValue : DateTimeOffset.FromUnixTimeMilliseconds(timestamp),
            Priority = priority,
            Properties = properties,
            MessageDelivery = new MessageDelivery(ConsumerId, messageId),
            RoutingType = routingType,
            GroupId = groupId,
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
    private static int DecodeProperties(ReadOnlySpan<byte> buffer,
        out IReadOnlyDictionary<string, object?> properties,
        out RoutingType? routingType,
        out string? groupId)
    {
        routingType = null;
        groupId = null;
        properties = ReadOnlyDictionary<string, object?>.Empty;
        
        var readBytes = ArtemisBinaryConverter.ReadByte(buffer, out var isNotNull);
        if (isNotNull == DataConstants.NotNull)
        {
            readBytes += ArtemisBinaryConverter.ReadInt32(buffer[readBytes..], out var count);
            var mutableProperties = new Dictionary<string, object?>(count);
            for (var i = 0; i < count; i++)
            {
                readBytes += ArtemisBinaryConverter.ReadSimpleString(buffer[readBytes..], out var key);

                if (key == MessageHeaders.RoutingType)
                {
                    readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var type);
                    if (type == DataConstants.Byte)
                    {
                        readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var routingTypeByte);
                        routingType = (RoutingType) routingTypeByte;
                    }
                }
                else if (key == MessageHeaders.GroupId)
                {
                    readBytes += ArtemisBinaryConverter.ReadByte(buffer[readBytes..], out var type);
                    if (type == DataConstants.String)
                    {
                        readBytes += ArtemisBinaryConverter.ReadSimpleString(buffer[readBytes..], out groupId);
                    }
                }
                else
                {
                    readBytes += ArtemisBinaryConverter.ReadNullableObject(buffer[readBytes..], out var obj);
                    mutableProperties.Add(key, obj);
                }
            }

            properties = mutableProperties.ToFrozenDictionary();
        }

        return readBytes;
    }
}