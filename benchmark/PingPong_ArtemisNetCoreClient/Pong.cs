using ActiveMQ.Artemis.Core.Client;

namespace PingPong_ArtemisNetCoreClient
{
    public class Pong : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IProducer _producer;
        private readonly IConsumer _consumer;
        private readonly Task _consumerLoopTask;
        private readonly CancellationTokenSource _cts;

        private Pong(IConnection connection, ISession session, IProducer producer, IConsumer consumer)
        {
            _connection = connection;
            _session = session;
            _producer = producer;
            _consumer = consumer;
            _cts = new CancellationTokenSource();

            _consumerLoopTask = ConsumerLoop();
        }

        public static async Task<Pong> CreateAsync(Endpoint endpoint)
        {
            var connectionFactory = new ConnectionFactory();
            var connection = await connectionFactory.CreateAsync(endpoint);
            var session = await connection.CreateSessionAsync();
            var producer = await session.CreateProducerAsync(new ProducerConfiguration { Address = "pong" });
            var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration { QueueName = "ping" });
            return new Pong(connection, session, producer, consumer);
        }

        private Task ConsumerLoop()
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        var pingMsg = await _consumer.ReceiveMessageAsync(_cts.Token);
                        var pongMessage = new Message { Body = "Pong"u8.ToArray(), Durable = false };
                        // ReSharper disable once MethodHasAsyncOverload
                        _producer.SendMessage(pongMessage);
                        await _consumer.AcknowledgeAsync(pingMsg.MessageDelivery, _cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            await _consumerLoopTask;
            await _consumer.DisposeAsync();
            await _producer.DisposeAsync();
            await _session.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}