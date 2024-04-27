namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct NullResponse : IIncomingPacket
{
    public readonly long CorrelationId;
    
    public NullResponse(ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            CorrelationId = -1;
        }
        else
        {
            _ = ArtemisBinaryConverter.ReadInt64(buffer, out CorrelationId);
        }
    }
}