namespace ActiveMQ.Artemis.Core.Client;

internal enum PacketType : byte
{
    CreateSessionMessage = unchecked((byte) -18),
    CreateSessionResponse = 31,
    SessionStart = 67,
    NullResponse = 21,
    SessionStop = 68,
    SessionCloseMessage = 69,
    CreateAddressMessage = unchecked((byte) -11),
    SessionBindingQueryMessage = 49,
    SessionBindingQueryResponseMessage = unchecked((byte) -22),
    CreateQueueMessage = unchecked((byte) -12),
    SessionQueueQueryMessage = 45,
    SessionQueueQueryResponseMessage = unchecked((byte) -14),
    SessionCreateConsumerMessage = 40,
    SessionConsumerCloseMessage = 74,
    CreateProducerMessage = unchecked((byte) -20),
    RemoveProducerMessage = unchecked((byte) -21)
}