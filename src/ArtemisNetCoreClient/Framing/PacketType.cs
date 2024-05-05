namespace ActiveMQ.Artemis.Core.Client.Framing;

internal enum PacketType : sbyte
{
    NullResponse = 21,
    Exception = 20,
    CreateSessionResponse = 31,
    SessionDeleteQueueMessage = 35,
    SessionCreateConsumerMessage = 40,
    SessionQueueQueryMessage = 45,
    SessionBindingQueryMessage = 49,
    SessionStart = 67,
    SessionConsumerFlowCreditMessage = 70,
    SessionSendMessage = 71,
    SessionStop = 68,
    SessionCloseMessage = 69,
    SessionReceiveMessage = 75,
    SessionConsumerCloseMessage = 74,
    CreateAddressMessage = -11,
    CreateQueueMessage = -12,
    SessionQueueQueryResponseMessage = -14,
    CreateSessionMessage = -18,
    CreateProducerMessage = -20,
    RemoveProducerMessage = -21,
    SessionBindingQueryResponseMessage = -22,
}