namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class CreateSessionResponseMessage : Packet
{
    public const byte Type = 31;
    public int ServerVersion { get; private set; }

    public override void Decode(ByteBuffer buffer)
    {
        ServerVersion = buffer.ReadInt();
    }
}