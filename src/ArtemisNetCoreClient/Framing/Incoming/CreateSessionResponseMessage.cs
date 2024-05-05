using System.Diagnostics;

namespace ActiveMQ.Artemis.Core.Client.Framing.Incoming;

internal readonly struct CreateSessionResponseMessage : IIncomingPacket
{
    public readonly int ServerVersion;
    
    public CreateSessionResponseMessage(ReadOnlySpan<byte> buffer)
    {
        var readBytes = ArtemisBinaryConverter.ReadInt32(buffer, out ServerVersion);
        Debug.Assert(readBytes == buffer.Length, $"Expected to read {buffer.Length} bytes but got {readBytes}");
    }
}