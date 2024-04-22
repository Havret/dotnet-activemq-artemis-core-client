namespace ActiveMQ.Artemis.Core.Client;

internal class IdGenerator(long startId)
{
    public long GenerateId() => Interlocked.Increment(ref startId);
}