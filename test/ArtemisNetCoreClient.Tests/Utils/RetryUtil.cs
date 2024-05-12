namespace ActiveMQ.Artemis.Core.Client.Tests.Utils;

public static class RetryUtil
{
    public static async Task<T> RetryUntil<T>(Func<Task<T>> func, Func<T, bool> until, CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = await func();
            if (until(result))
                return result;
            if (cancellationToken.IsCancellationRequested)
                return result;
                
            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
        }
    }
}