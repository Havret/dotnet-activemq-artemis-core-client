namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct CreateSessionMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.CreateSessionMessage;
    
    public string Name { get; init; }
    public long SessionChannelId { get; init; }
    public int Version { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public int MinLargeMessageSize { get; init; }
    public bool Xa { get; init; }
    public bool AutoCommitSends { get; init; }
    public bool AutoCommitAcks { get; init; }
    public bool PreAcknowledge { get; init; }
    public int WindowSize { get; init; }
    public string? DefaultAddress { get; init; }
    public string? ClientId { get; init; }

    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        
        byteCount += ArtemisBitConverter.GetStringByteCount(Name);
        byteCount += sizeof(long); // SessionChannelId
        byteCount += sizeof(int); // Version
        byteCount += ArtemisBitConverter.GetNullableStringByteCount(Username);
        byteCount += ArtemisBitConverter.GetNullableStringByteCount(Password);
        byteCount += sizeof(int); // MinLargeMessageSize
        byteCount += sizeof(bool); // Xa
        byteCount += sizeof(bool); // AutoCommitSends
        byteCount += sizeof(bool); // AutoCommitAcks
        byteCount += sizeof(int); // WindowSize
        byteCount += sizeof(bool); // PreAcknowledge
        byteCount += ArtemisBitConverter.GetNullableStringByteCount(DefaultAddress);
        byteCount += ArtemisBitConverter.GetNullableStringByteCount(ClientId);

        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBitConverter.WriteString(ref buffer.GetOffset(offset), Name);
        offset += ArtemisBitConverter.WriteInt64(ref buffer.GetOffset(offset), SessionChannelId);
        offset += ArtemisBitConverter.WriteInt32(ref buffer.GetOffset(offset), Version);
        // offset += ArtemisBitConverter.WriteNullableString(ref buffer.GetOffset(offset), Username);
        // offset += ArtemisBitConverter.WriteNullableString(ref buffer.GetOffset(offset), Password);
        offset += ArtemisBitConverter.WriteInt32(ref buffer.GetOffset(offset), MinLargeMessageSize);
        // offset += ArtemisBitConverter.WriteBool(ref buffer.GetOffset(offset), Xa);
        // offset += ArtemisBitConverter.WriteBool(ref buffer.GetOffset(offset), AutoCommitSends);
        // offset += ArtemisBitConverter.WriteBool(ref buffer.GetOffset(offset), AutoCommitAcks);
        offset += ArtemisBitConverter.WriteInt32(ref buffer.GetOffset(offset), WindowSize);
        // offset += ArtemisBitConverter.WriteBool(ref buffer.GetOffset(offset), PreAcknowledge);
        // offset += ArtemisBitConverter.WriteNullableString(ref buffer.GetOffset(offset), DefaultAddress);
        // offset += ArtemisBitConverter.WriteNullableString(ref buffer.GetOffset(offset), ClientId);

        return offset;
    }
}

internal class CreateSessionMessageV2 : Packet
{
    public const byte Type = unchecked((byte) -18);
    public required string Name { get; init; }
    public long SessionChannelId { get; init; }
    public int Version { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public int MinLargeMessageSize { get; init; }
    public bool Xa { get; init; }
    public bool AutoCommitSends { get; init; }
    public bool AutoCommitAcks { get; init; }
    public bool PreAcknowledge { get; init; }
    public int WindowSize { get; init; }
    public string? DefaultAddress { get; init; }
    public string? ClientId { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteString(Name);
        buffer.WriteLong(SessionChannelId);
        buffer.WriteInt(Version);
        buffer.WriteNullableString(Username);
        buffer.WriteNullableString(Password);
        buffer.WriteInt(MinLargeMessageSize);
        buffer.WriteBool(Xa);
        buffer.WriteBool(AutoCommitSends);
        buffer.WriteBool(AutoCommitAcks);
        buffer.WriteInt(WindowSize);
        buffer.WriteBool(PreAcknowledge);
        buffer.WriteNullableString(DefaultAddress);
        buffer.WriteNullableString(ClientId);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}