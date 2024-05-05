using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client;

internal interface IChannel
{
    void OnPacket(in InboundPacket packet);
}