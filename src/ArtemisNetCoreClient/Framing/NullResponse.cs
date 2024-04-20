namespace ActiveMQ.Artemis.Core.Client.Framing;

internal readonly struct NullResponse2 : IIncomingPacket
{
    public readonly long CorrelationId;
    
    public NullResponse2(ReadOnlySpan<byte> buffer)
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

internal class NullResponse : Packet
{
    public const byte Type = 21;
    public override bool IsResponse => true;
    public override void Encode(ByteBuffer buffer)
    {
    }

    public override void Decode(ByteBuffer buffer)
    {
        if (buffer.ReadableBytes() >= sizeof(long))
        {
            CorrelationId = buffer.ReadLong();
        }
        else
        {
            CorrelationId = -1;
        }
    }
}