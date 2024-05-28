using Apache.NMS;
using Apache.NMS.AMQP;

namespace Throughput_NMS_AMQP;

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

        var consumer = await session.CreateConsumerAsync(await session.GetQueueAsync("throughput"));
        return new Consumer(connection, session, consumer);
    }

    public async Task StartConsumingAsync(int messages)
    {
        for (var i = 0; i < messages; i++)
        {
            var message = await _consumer.ReceiveAsync();
            await message.AcknowledgeAsync();
        }
    }

    public void Dispose()
    {
        _consumer.Dispose();
        _session.Dispose();
        _connection.Dispose();
    }
}