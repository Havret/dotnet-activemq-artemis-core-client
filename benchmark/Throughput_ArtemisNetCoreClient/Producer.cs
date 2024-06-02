using System.Buffers;
using ActiveMQ.Artemis.Core.Client;

namespace Throughput_ArtemisNetCoreClient;

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

        var producer = await session.CreateProducerAsync(new ProducerConfiguration { Address = "throughput" });
        return new Producer(connection, session, producer);
    }

    public async Task SendMessages(int messages, int payloadSize)
    {
        for (var i = 0; i < messages; i++)
        {
            var lastMessage = i == messages - 1;
            if (!lastMessage)
            {
                var message = new Message
                {
                    Body = GenerateRandomData(payloadSize),
                    Durable = false
                };
                // ReSharper disable once MethodHasAsyncOverload
                _producer.SendMessage(message);
            }
            else
            {
                // The last message should not be sent in a fire-and-forget manner as we want to ensure that all messages are received
                // by the broker before leaving this method.
                var message = new Message
                {
                    Body = GenerateRandomData(payloadSize),
                    Durable = true
                };
                await _producer.SendMessageAsync(message);
            }
        }
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