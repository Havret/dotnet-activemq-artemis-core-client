namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct CreateAddressMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.CreateAddressMessage;
    
    public required string Address { get; init; }
    public required RoutingType[] RoutingTypes { get; init; }
    public required bool RequiresResponse { get; init; }
    public required bool AutoCreated { get; init; }

    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(Address);
        byteCount += sizeof(int); // RoutingTypes.Length
        byteCount += RoutingTypes.Length * sizeof(byte); // RoutingTypes
        byteCount += sizeof(bool); // RequiresResponse
        byteCount += sizeof(bool); // AutoCreated

        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetReference(), Address);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), RoutingTypes.Length);
        foreach (var routingType in RoutingTypes)
        {
            offset += ArtemisBinaryConverter.WriteByte(ref buffer.GetOffset(offset), (byte) routingType);
        }
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), RequiresResponse);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), AutoCreated);

        return offset;
    }
}

internal class CreateAddressMessage2 : Packet
{
    public const byte Type = unchecked((byte) -11);
    public required string Address { get; init; }
    public required RoutingType[] RoutingTypes { get; init; }
    public required bool RequiresResponse { get; init; }
    public required bool AutoCreated { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteByteString(Address);
        buffer.WriteInt(RoutingTypes.Length);
        foreach (var routingType in RoutingTypes)
        {
            buffer.WriteByte((byte) routingType);
        }
        buffer.WriteBool(RequiresResponse);
        buffer.WriteBool(AutoCreated);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}