namespace ActiveMQ.Artemis.Core.Client.Tests.Utils;

internal static class DateTimeOffsetExtensions
{
    public static DateTimeOffset DropTicsPrecision(this DateTimeOffset dateTime)
    {
        return new DateTime(dateTime.Ticks / TimeSpan.TicksPerMillisecond  * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
    }
}