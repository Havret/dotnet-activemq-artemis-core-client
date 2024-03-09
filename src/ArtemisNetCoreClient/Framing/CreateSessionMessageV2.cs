namespace ActiveMQ.Artemis.Core.Client.Framing;

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
        throw new NotImplementedException();
    }
}