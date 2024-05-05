using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct SessionBindingQueryMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionBindingQueryMessage;
    public required string Address { get; init; }
    public int GetRequiredBufferSize()
    {
        int byteCount = 0;
        byteCount += ArtemisBinaryConverter.GetSimpleStringByteCount(Address);
        return byteCount;
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        offset += ArtemisBinaryConverter.WriteSimpleString(ref buffer.GetReference(), Address);
        return offset;
    }
}