using ActiveMQ.Artemis.Core.Client.Tests.Utils.Logging;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests.Utils;

public class TestFixture : IAsyncDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CancellationTokenSource _cts;

    public static async Task<TestFixture> CreateAsync(ITestOutputHelper testOutputHelper)
    {
        await Task.Delay(0);
        return new TestFixture(testOutputHelper);
    }

    private TestFixture(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    }
    
    public async Task<ISession> CreateSessionAsync()
    {
        var sessionFactory = new SessionFactory
        {
            LoggerFactory = new XUnitLoggerFactory(_testOutputHelper)
        };
        return await sessionFactory.CreateAsync(GetEndpoint(), CancellationToken);
    }
    
    public async Task<IConnection> CreateConnectionAsync()
    {
        var connectionFactory = new ConnectionFactory
        {
            LoggerFactory = new XUnitLoggerFactory(_testOutputHelper)
        };
        return await connectionFactory.CreateAsync(GetEndpoint(), CancellationToken);
    }
    
    public static Endpoint GetEndpoint()
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