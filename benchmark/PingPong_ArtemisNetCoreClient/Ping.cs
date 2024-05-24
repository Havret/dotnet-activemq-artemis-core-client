using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client;

namespace PingPong_ArtemisNetCoreClient
{
    public class Ping : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IProducer _producer;
        private readonly IConsumer _consumer;
        private readonly Stopwatch _stopwatch;
        private int _numberOfMessages;
        private int _skipMessages;
        private int _counter;
        private TaskCompletionSource<Stats> _tsc;
        private readonly Task _consumerLoopTask;
        private readonly CancellationTokenSource _cts;

        private Ping(IConnection connection, ISession session, IProducer producer, IConsumer consumer)
        {
            _connection = connection;
            _session = session;
            _producer = producer;
            _consumer = consumer;
            _stopwatch = new Stopwatch();
  
            _cts = new CancellationTokenSource();

            _consumerLoopTask = ConsumerLoop();
        }

        public static async Task<Ping> CreateAsync(Endpoint endpoint)
        {
            var connectionFactory = new ConnectionFactory();
            var connection = await connectionFactory.CreateAsync(endpoint);
            var session = await connection.CreateSessionAsync();
            
            var producer = await session.CreateProducerAsync(new ProducerConfiguration { Address = "ping" });
            var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration { QueueName = "pong" });
            return new Ping(connection, session, producer, consumer);
        }

        public Task<Stats> Start(int numberOfMessages, int skipMessages)
        {
            _numberOfMessages = numberOfMessages;
            _skipMessages = skipMessages;
            _stopwatch.Start();
            _tsc = new TaskCompletionSource<Stats>();
            var pingMessage = new Message { Body = "Ping"u8.ToArray() };
            _producer.SendMessageAsync(pingMessage);
            return _tsc.Task;
        }

        private Task ConsumerLoop()
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        var msg = await _consumer.ReceiveMessageAsync(_cts.Token);
                        if (_skipMessages > 0)
                            _skipMessages--;
                        else
                            _counter++;

                        if (_counter == _numberOfMessages)
                        {
                            _stopwatch.Stop();
                            _tsc.TrySetResult(new Stats { MessagesCount = _counter, Elapsed = _stopwatch.Elapsed });
                        }
                        else
                        {
                            var pingMessage = new Message { Body = "Ping"u8.ToArray() };
                            await _producer.SendMessageAsync(pingMessage, _cts.Token);
                        }
                        await _consumer.AcknowledgeAsync(msg.MessageDelivery, _cts.Token);
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