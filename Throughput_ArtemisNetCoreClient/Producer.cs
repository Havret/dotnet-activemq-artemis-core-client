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

    public Task SendMessagesAsync(int messages, int payloadSize)
    {
        return Task.Run(async () =>
        {
            for (var i = 0; i < messages; i++)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(payloadSize);
                try
                {
                    FillRandomData(buffer.AsSpan(0, payloadSize));
                    var message = new Message { Body = new ReadOnlyMemory<byte>(buffer, 0, payloadSize), };
                    await _producer.SendMessageAsync(message);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        });
    }

    private void FillRandomData(Span<byte> buffer)
    {
        _random.NextBytes(buffer);
    }

    public async ValueTask DisposeAsync()
    {
        await _producer.DisposeAsync();
        await _session.DisposeAsync();
        await _connection.DisposeAsync();
    }
}