using System.Diagnostics;
using Apache.NMS;

namespace PingPong_NMS.AMQP
{
    public class Ping: IDisposable
    {
        private readonly IConnection _connection;
        private readonly ISession _session;
        private readonly IMessageProducer _messageProducer;
        private readonly IMessageConsumer _messageConsumer;
        private readonly Stopwatch _stopwatch;
        private TaskCompletionSource<Stats>? _tsc;
        private int _numberOfMessages;
        private int _skipMessages;
        private int _counter;

        public Ping(IConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.CreateConnection();
            _session = _connection.CreateSession();
            _messageProducer = _session.CreateProducer(_session.GetQueue("ping"));
            _messageConsumer = _session.CreateConsumer(_session.GetQueue("pong"));
            _messageConsumer.Listener += OnMessage;
            _stopwatch = new Stopwatch();
            _connection.Start();
        }

        private void OnMessage(IMessage message)
        {
            if (_skipMessages > 0)
                _skipMessages--;
            else
                _counter++;

            if (_counter == _numberOfMessages)
            {
                _stopwatch.Stop();
                _tsc?.SetResult(new Stats { MessagesCount = _counter, Elapsed = _stopwatch.Elapsed });                
            }
            else
            {
                var pingMessage = _messageProducer.CreateBytesMessage("Ping"u8.ToArray());
                pingMessage.NMSDeliveryMode = MsgDeliveryMode.NonPersistent;
                _messageProducer.Send(pingMessage);
            }
        }

        public Task<Stats> Start(int numberOfMessages, int skipMessages)
        {
            _numberOfMessages = numberOfMessages;
            _skipMessages = skipMessages;
            _tsc = new TaskCompletionSource<Stats>(TaskCreationOptions.RunContinuationsAsynchronously);
            _stopwatch.Start();
            var pingMessage = _messageProducer.CreateBytesMessage("Ping"u8.ToArray());
            pingMessage.NMSDeliveryMode = MsgDeliveryMode.NonPersistent;
            _messageProducer.Send(pingMessage);
            return _tsc.Task;
        }

        public void Dispose()
        {
            _messageConsumer.Dispose();
            _messageProducer.Dispose();
            _session.Dispose();
            _connection.Dispose();
        }
    }
}