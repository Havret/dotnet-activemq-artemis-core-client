namespace ActiveMQ.Artemis.Core.Client.Utils;

internal class IdGenerator(long startId)
{
    public long GenerateId() => Interlocked.Increment(ref startId);
}