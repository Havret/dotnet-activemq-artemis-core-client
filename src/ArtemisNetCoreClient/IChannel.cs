namespace ActiveMQ.Artemis.Core.Client;

internal interface IChannel
{
    void OnPacket(in InboundPacket packet);
}