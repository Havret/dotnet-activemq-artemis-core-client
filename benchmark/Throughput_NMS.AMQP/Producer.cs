using Apache.NMS;
using Apache.NMS.AMQP;

namespace Throughput_NMS_AMQP;

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

        var producer = await session.CreateProducerAsync(await session.GetQueueAsync("throughput"));
        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        return new Producer(connection, session, producer);
    }

    public async Task SendMessagesAsync(int messages, int payloadSize)
    {
        for (var i = 0; i < messages; i++)
        {
            var pingMessage = await _producer.CreateBytesMessageAsync(GenerateRandomData(payloadSize));
            
            var lastMessage = i == messages - 1;
            
            // The last message should be persistent.
            // For persistent messages NMS.AMQP will await for the confirmation that the message was received by the broker.
            // If the last message was received by the broker, we can be sure that all messages were received.
            pingMessage.NMSDeliveryMode = lastMessage 
                ? MsgDeliveryMode.Persistent 
                : MsgDeliveryMode.NonPersistent;

            await _producer.SendAsync(pingMessage);
        }
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