namespace ActiveMQ.Artemis.Core.Client.InternalUtilities;

internal class IdGenerator(long startId)
{
    public long GenerateId() => Interlocked.Increment(ref startId);
}