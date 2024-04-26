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
    public int WindowSize { get; init; }
    public bool PreAcknowledge { get; init; }
    public string? DefaultAddress { get; init; }
    public string? ClientId { get; init; }

    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        
        byteCount += ArtemisBinaryConverter.GetStringByteCount(Name);
        byteCount += sizeof(long); // SessionChannelId
        byteCount += sizeof(int); // Version
        byteCount += ArtemisBinaryConverter.GetNullableStringByteCount(Username);
        byteCount += ArtemisBinaryConverter.GetNullableStringByteCount(Password);
        byteCount += sizeof(int); // MinLargeMessageSize
        byteCount += sizeof(bool); // Xa
        byteCount += sizeof(bool); // AutoCommitSends
        byteCount += sizeof(bool); // AutoCommitAcks
        byteCount += sizeof(int); // WindowSize
        byteCount += sizeof(bool); // PreAcknowledge
        byteCount += ArtemisBinaryConverter.GetNullableStringByteCount(DefaultAddress);
        byteCount += ArtemisBinaryConverter.GetNullableStringByteCount(ClientId);

        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        
        offset += ArtemisBinaryConverter.WriteString(ref buffer.GetOffset(offset), Name);
        offset += ArtemisBinaryConverter.WriteInt64(ref buffer.GetOffset(offset), SessionChannelId);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), Version);
        offset += ArtemisBinaryConverter.WriteNullableString(ref buffer.GetOffset(offset), Username);
        offset += ArtemisBinaryConverter.WriteNullableString(ref buffer.GetOffset(offset), Password);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), MinLargeMessageSize);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), Xa);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), AutoCommitSends);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), AutoCommitAcks);
        offset += ArtemisBinaryConverter.WriteInt32(ref buffer.GetOffset(offset), WindowSize);
        offset += ArtemisBinaryConverter.WriteBool(ref buffer.GetOffset(offset), PreAcknowledge);
        offset += ArtemisBinaryConverter.WriteNullableString(ref buffer.GetOffset(offset), DefaultAddress);
        offset += ArtemisBinaryConverter.WriteNullableString(ref buffer.GetOffset(offset), ClientId);

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