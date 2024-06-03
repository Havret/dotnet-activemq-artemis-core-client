using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client;

namespace Latency_ArtemisNetCoreClient;

public class Producer : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly ISession _session;
    private readonly IProducer _producer;
    private readonly Random _random;

    private Producer(IConnection connection, ISession session, IProducer producer)
    {
        _connection = connection;
        _session = session;
        _producer = producer;
        _random = new Random();
    }

    public static async Task<Producer> CreateAsync(Endpoint endpoint)
    {
        var connectionFactory = new ConnectionFactory();
        var connection = await connectionFactory.CreateAsync(endpoint);
        var session = await connection.CreateSessionAsync();

        var producer = await session.CreateProducerAsync(new ProducerConfiguration { Address = "latency" });
        return new Producer(connection, session, producer);
    }

    public Task SendMessagesAsync(int messages, int payloadSize)
    {
        return Task.Run(async () =>
        {
            for (var i = 0; i < messages; i++)
            {
                var timestamp = Stopwatch.GetTimestamp(); // Get high precision timestamp
                var message = new Message
                {
                    Body = GenerateRandomData(payloadSize),
                    Properties = new Dictionary<string, object?>
                    {
                        ["Timestamp"] = timestamp
                    },
                    Durable = true
                };
                await _producer.SendMessageAsync(message);
            }
        });
    }
    
    private byte[] GenerateRandomData(int size)
    {
        var data = new byte[size];
        _random.NextBytes(data);
        return data;
    }

    public async ValueTask DisposeAsync()
    {
        await _producer.DisposeAsync();
        await _session.DisposeAsync();
        await _connection.DisposeAsync();
    }
}