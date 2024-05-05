namespace ActiveMQ.Artemis.Core.Client.Exceptions;

public class ActiveMQException(ActiveMQExceptionType type, string message) : Exception(message)
{
    public ActiveMQExceptionType Type { get; } = type;
}

public class ActiveMQNonExistentQueueException() : ActiveMQException(ActiveMQExceptionType.QueueDoesNotExist, string.Empty)
;