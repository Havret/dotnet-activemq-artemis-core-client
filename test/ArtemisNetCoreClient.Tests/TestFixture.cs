namespace ActiveMQ.Artemis.Core.Client.Tests;

public class TestFixture : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts;

    public static async Task<TestFixture> CreateAsync()
    {
        await Task.Delay(0);
        return new TestFixture();
    }

    private TestFixture()
    {
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    }
    
    public Endpoint GetEndpoint()
    {
        var userName = Environment.GetEnvironmentVariable("ARTEMIS_USERNAME") ?? "artemis";
        var password = Environment.GetEnvironmentVariable("ARTEMIS_PASSWORD") ?? "artemis";
        var host = Environment.GetEnvironmentVariable("ARTEMIS_HOST") ?? "localhost";
        var port = int.Parse(Environment.GetEnvironmentVariable("ARTEMIS_PORT") ?? "5445");
        return new Endpoint
        {
            Host = host,
            Port = port,
            User = userName,
            Password = password,
        };
    }
    
    public CancellationToken CancellationToken => _cts.Token;
    
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}