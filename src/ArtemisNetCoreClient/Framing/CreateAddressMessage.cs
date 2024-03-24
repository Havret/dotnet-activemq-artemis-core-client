namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class CreateAddressMessage : Packet
{
    public const byte Type = unchecked((byte) -11);
    public required string Address { get; init; }
    public required RoutingType[] RoutingTypes { get; init; }
    public required bool RequiresResponse { get; init; }
    public required bool AutoCreated { get; init; }

    public override void Encode(ByteBuffer buffer)
    {
        buffer.WriteAmqString(Address);
        buffer.WriteInt(RoutingTypes.Length);
        foreach (var routingType in RoutingTypes)
        {
            buffer.WriteByte((byte) routingType);
        }
        buffer.WriteBool(RequiresResponse);
        buffer.WriteBool(AutoCreated);
    }

    public override void Decode(ByteBuffer buffer)
    {
    }
}