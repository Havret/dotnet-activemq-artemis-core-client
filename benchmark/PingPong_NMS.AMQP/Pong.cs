using Apache.NMS;

namespace PingPong_NMS.AMQP;

public class Pong : IDisposable
{
    private readonly IConnection _connection;
    private readonly ISession _session;
    private readonly IMessageProducer _messageProducer;
    private readonly IMessageConsumer _messageConsumer;

    public Pong(IConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
        _session = _connection.CreateSession();
        _messageProducer = _session.CreateProducer(_session.GetQueue("pong"));
        _messageConsumer = _session.CreateConsumer(_session.GetQueue("ping"));
        _messageConsumer.Listener += OnMessage;
        _connection.Start();
    }

    private void OnMessage(IMessage message)
    {
        var pongMessage = _session.CreateBytesMessage("Pong"u8.ToArray());
        _messageProducer.Send(pongMessage);
    }

    public void Dispose()
    {
        _messageConsumer.Dispose();
        _messageProducer.Dispose();
        _session.Dispose();
        _connection.Dispose();
    }
}