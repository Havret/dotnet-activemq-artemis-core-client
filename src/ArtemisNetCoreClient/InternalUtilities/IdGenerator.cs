namespace ActiveMQ.Artemis.Core.Client.InternalUtilities;

internal class IdGenerator(long startId)
{
    private long _startId = startId - 1;

    public long GenerateId()
    {
        return Interlocked.Increment(ref _startId);
    }
}