namespace ActiveMQ.Artemis.Core.Client.Exceptions;

public class ActiveMQException : Exception
{
    public ActiveMQException(int code, string message) : base(message)
    {
        // TODO: Handle Exception Code as Exception Type
    }
}