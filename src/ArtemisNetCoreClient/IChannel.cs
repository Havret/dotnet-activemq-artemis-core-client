namespace ActiveMQ.Artemis.Core.Client;

internal interface IChannel
{
    void OnPacket(ref readonly InboundPacket packet);
}