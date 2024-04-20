namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct CreateSessionResponseMessage2 : IIncomingPacket
{
    public readonly int ServerVersion;
    
    public CreateSessionResponseMessage2(ReadOnlySpan<byte> buffer)
    {
        ArtemisBinaryConverter.ReadInt32(buffer, out ServerVersion);
    }
}

internal class CreateSessionResponseMessage : Packet
{
    public const byte Type = 31;
    public int ServerVersion { get; private set; }

    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
        ServerVersion = buffer.ReadInt();
    }
}