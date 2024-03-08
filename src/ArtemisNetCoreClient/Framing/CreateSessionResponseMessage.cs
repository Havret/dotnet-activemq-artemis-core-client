namespace ActiveMQ.Artemis.Core.Client.Framing;

internal class CreateSessionResponseMessage : Packet
{
    public long ChannelId { get; private set; }
    public int ServerVersion { get; private set; }
    
    public override byte Type { get; } = 31;
    
    public override void Encode(ByteBuffer buffer)
    {
        throw new NotImplementedException();
    }

    public override void Decode(ByteBuffer buffer)
    {
        _ = buffer.ReadByte(); // type
        ChannelId = buffer.ReadLong();
        ServerVersion = buffer.ReadInt();
    }
}