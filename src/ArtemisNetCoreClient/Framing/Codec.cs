namespace ActiveMQ.Artemis.Core.Client.Framing;

internal static class Codec
{
    public static void Encode(ByteBuffer buffer, Packet packet, long channelId)
    {
        buffer.WriteInt(0); // The length gets filled in at the end

        var type = packet switch
        {
            CreateSessionMessageV2 => CreateSessionMessageV2.Type,
            _ => throw new ArgumentOutOfRangeException(nameof(packet), packet, $"{packet.GetType()} is not supported for encoding")
        };
        buffer.WriteByte(type);
        buffer.WriteLong(channelId);
        
        packet.Encode(buffer);
        
        buffer.WriteSize();
    }

    public static (Packet packet, long channelId) Decode(ByteBuffer buffer)
    {
        var type = buffer.ReadByte();
        var channelId = buffer.ReadLong();

        Packet packet = type switch
        {
            CreateSessionResponseMessage.Type => new CreateSessionResponseMessage(),
            _ => throw new ArgumentOutOfRangeException($"Type {type} is not supported for decoding")
        };
        
        packet.Decode(buffer);

        return (packet, channelId);
    }
}