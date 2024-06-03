using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.AMQP;

namespace Latency_NMS.AMQP;

public class Consumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly ISession _session;
    private readonly IMessageConsumer _consumer;

    private Consumer(IConnection connection, ISession session, IMessageConsumer consumer)
    {
        _connection = connection;
        _session = session;
        _consumer = consumer;
    }

    public static async Task<Consumer> CreateAsync(NmsConnectionFactory connectionFactory)
    {
        var connection = await connectionFactory.CreateConnectionAsync();
        var session = await connection.CreateSessionAsync();
        await connection.StartAsync();

        var consumer = await session.CreateConsumerAsync(await session.GetQueueAsync("latency"));
        return new Consumer(connection, session, consumer);
    }

    public Task<double[]> StartConsumingAsync(int messages)
    {
        return Task.Run(async () =>
        {
            var latencies = new double[messages];

            for (var i = 0; i < messages; i++)
            {
                var message = await _consumer.ReceiveAsync();
                var receiveTimestamp = Stopwatch.GetTimestamp();
                var sendTimestamp = (long) message.Properties["Timestamp"];
                latencies[i] = (receiveTimestamp - sendTimestamp) * 1_000_000.0 / Stopwatch.Frequency;
                // ReSharper disable once MethodHasAsyncOverload
                message.Acknowledge();
            }

            return latencies;
        });
    }

    public void Dispose()
    {
        _consumer.Dispose();
        _session.Dispose();
        _connection.Dispose();
    }
}