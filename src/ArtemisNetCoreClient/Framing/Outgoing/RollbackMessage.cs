using ActiveMQ.Artemis.Core.Client.InternalUtilities;

namespace ActiveMQ.Artemis.Core.Client.Framing.Outgoing;

internal readonly struct RollbackMessage : IOutgoingPacket
{
    public PacketType PacketType => PacketType.SessionRollbackMessage;
    
    public required bool ConsiderLastMessageAsDelivered { get; init; }
    
    public int GetRequiredBufferSize()
    {
        return sizeof(bool);
    }

    public int Encode(Span<byte> buffer)
    {
        return ArtemisBinaryConverter.WriteBool(ref buffer.GetReference(), ConsiderLastMessageAsDelivered);
    }
}