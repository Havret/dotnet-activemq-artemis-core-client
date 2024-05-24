namespace PingPong_ArtemisNetCoreClient;

public class Stats
{
    public int MessagesCount { get; init; }
    public TimeSpan Elapsed { get; init; }

    public override string ToString()
    {
        return $"Sent {MessagesCount} msgs in {Elapsed.TotalMilliseconds:F2}ms -- {MessagesCount / Elapsed.TotalSeconds:F2} msg/s";
    }
}