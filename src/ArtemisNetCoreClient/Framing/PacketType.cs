namespace ActiveMQ.Artemis.Core.Client.Framing;

internal enum PacketType : sbyte
{
    NullResponse = 21,
    Exception = 20,
    CreateSessionResponse = 31,
    SessionDeleteQueueMessage = 35,
    SessionCreateConsumerMessage = 40,
    SessionAcknowledgeMessage = 41,
    SessionQueueQueryMessage = 45,
    SessionBindingQueryMessage = 49,
    SessionStart = 67,
    SessionStop = 68,
    SessionCloseMessage = 69,
    SessionConsumerFlowCreditMessage = 70,
    SessionSendMessage = 71,
    SessionConsumerCloseMessage = 74,
    SessionReceiveMessage = 75,
    SessionIndividualAcknowledgeMessage = 81,
    CreateAddressMessage = -11,
    CreateQueueMessage = -12,
    SessionQueueQueryResponseMessage = -14,
    CreateSessionMessage = -18,
    CreateProducerMessage = -20,
    RemoveProducerMessage = -21,
    SessionBindingQueryResponseMessage = -22,

}