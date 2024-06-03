using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.AMQP;

namespace Latency_NMS.AMQP;

public class Producer : IDisposable
{
    private readonly IConnection _connection;
    private readonly ISession _session;
    private readonly IMessageProducer _producer;
    private readonly Random _random;

    private Producer(IConnection connection, ISession session, IMessageProducer producer)
    {
        _connection = connection;
        _session = session;
        _producer = producer;
        _random = new Random();
    }

    public static async Task<Producer> CreateAsync(NmsConnectionFactory connectionFactory)
    {
        var connection = await connectionFactory.CreateConnectionAsync();
        var session = await connection.CreateSessionAsync();

        var producer = await session.CreateProducerAsync(await session.GetQueueAsync("latency"));
        return new Producer(connection, session, producer);
    }

    public Task SendMessagesAsync(int messages, int payloadSize)
    {
        return Task.Run(async () =>
        {
            for (var i = 0; i < messages; i++)
            {
                var timestamp = Stopwatch.GetTimestamp(); // Get high precision timestamp
                var pingMessage = await _producer.CreateBytesMessageAsync(GenerateRandomData(payloadSize));
                // pingMessage.NMSDeliveryMode = MsgDeliveryMode.Persistent;
                pingMessage.Properties["Timestamp"] = timestamp;
                
                // ReSharper disable once MethodHasAsyncOverload
                _producer.Send(pingMessage);
            }
        });
    }

    private byte[] GenerateRandomData(int size)
    {
        byte[] data = new byte[size];
        _random.NextBytes(data);
        return data;
    }

    public void Dispose()
    {
        _producer.Dispose();
        _session.Dispose();
        _connection.Dispose();
    }
}