namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct CreateSessionResponseMessage : IIncomingPacket
{
    public readonly int ServerVersion;
    
    public CreateSessionResponseMessage(ReadOnlySpan<byte> buffer)
    {
        ArtemisBinaryConverter.ReadInt32(buffer, out ServerVersion);
    }
}