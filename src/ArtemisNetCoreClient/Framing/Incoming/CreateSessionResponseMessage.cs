using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

internal readonly struct CreateSessionResponseMessage : IIncomingPacket
{
    public readonly int ServerVersion;
    
    public CreateSessionResponseMessage(ReadOnlySpan<byte> buffer)
    {
        ArtemisBinaryConverter.ReadInt32(buffer, out ServerVersion);
    }
}