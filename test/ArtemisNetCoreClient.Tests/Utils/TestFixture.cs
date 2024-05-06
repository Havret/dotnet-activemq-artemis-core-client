using ActiveMQ.Artemis.Core.Client.Tests.Utils.Logging;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests.Utils;

public class TestFixture : IAsyncDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CancellationTokenSource _cts;
    private IConnection? _testConnection;
    private readonly List<string> _queues = new();

    public static async Task<TestFixture> CreateAsync(ITestOutputHelper testOutputHelper)
    {
        await Task.Delay(0);
        return new TestFixture(testOutputHelper);
    }

    private TestFixture(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _cts = new CancellationTokenSource(Timeout);
    }
    
    /// <summary>
    /// Create an address with random name that can be used for testing.
    /// </summary>
    public async Task<string> CreateAddressAsync(RoutingType routingType = RoutingType.Multicast)
    {
        var connection = await GetTestConnectionAsync();
        var session = await connection.CreateSessionAsync(CancellationToken);
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, [routingType], CancellationToken);
        return addressName;
    }
    
    /// <summary>
    /// Create a queue with random name that can be used for testing.
    /// </summary>
    public async Task<string> CreateQueueAsync(string addressName, RoutingType routingType = RoutingType.Multicast)
    {
        var connection = await GetTestConnectionAsync();
        var session = await connection.CreateSessionAsync(CancellationToken);
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueueAsync(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = routingType
        }, CancellationToken);
        _queues.Add(queueName);
        return queueName;
    }

    private async ValueTask<IConnection> GetTestConnectionAsync()
    {
        if (_testConnection != null)
        {
            return _testConnection;
        }
        _testConnection = await CreateConnectionAsync();
        return _testConnection;
    }

    private static TimeSpan Timeout
    {
        get
        {
#if DEBUG
            return TimeSpan.FromSeconds(60);
#else
            return TimeSpan.FromSeconds(5);
#endif
        }
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
        foreach (var queue in _queues)
        {
            try
            {
                var connection = await GetTestConnectionAsync();
                await using var session = await connection.CreateSessionAsync(CancellationToken);
                await session.DeleteQueueAsync(queue, CancellationToken);
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine($"Failed to delete queue {queue}: {e.Message}");
            }
        }
        if (_testConnection != null)
        {
            await _testConnection.DisposeAsync();
        }
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}