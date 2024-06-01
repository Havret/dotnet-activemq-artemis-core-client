using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client;

namespace Latency_ArtemisNetCoreClient;

public class Consumer : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly ISession _session;
    private readonly IConsumer _consumer;

    private Consumer(IConnection connection, ISession session, IConsumer consumer)
    {
        _connection = connection;
        _session = session;
        _consumer = consumer;
    }

    public static async Task<Consumer> CreateAsync(Endpoint endpoint)
    {
        var connectionFactory = new ConnectionFactory();
        var connection = await connectionFactory.CreateAsync(endpoint);
        var session = await connection.CreateSessionAsync();
            
        var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration { QueueName = "latency" });
        return new Consumer(connection, session, consumer);
    }
    
    public Task<double[]> StartConsumingAsync(int messages)
    {
        return Task.Run(async () =>
        {
            var latencies = new double[messages];

            for (var i = 0; i < messages; i++)
            {
                var message = await _consumer.ReceiveMessageAsync();
                var receiveTimestamp = Stopwatch.GetTimestamp();
                var sendTimestamp = (long) message.Properties["Timestamp"]!;
                latencies[i] = (receiveTimestamp - sendTimestamp) * 1_000_000.0 / Stopwatch.Frequency;
                
                // AMQP doesn't support waiting for the confirmation from the broker for message acknowledgment.
                // So if we want to compare apples to apples we need to use the fire-and-forget method of acknowledgment.
                _consumer.Acknowledge(message.MessageDelivery);
            }

            return latencies;
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _consumer.DisposeAsync();
        await _session.DisposeAsync();
        await _connection.DisposeAsync();
    }
}