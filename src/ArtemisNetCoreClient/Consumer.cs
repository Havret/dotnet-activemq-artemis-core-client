using System.Threading.Channels;

namespace ActiveMQ.Artemis.Core.Client;

internal class Consumer : IConsumer
{
    private readonly Session _session;
    private readonly ChannelReader<ReceivedMessage> _reader;
    private readonly ChannelWriter<ReceivedMessage> _writer;

    public Consumer(Session session)
    {
        _session = session;
        
        // TODO: Change to Bounded based on consumer credit
        var channel = Channel.CreateUnbounded<ReceivedMessage>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });
        _reader = channel.Reader;
        _writer = channel.Writer;
    }

    public required long ConsumerId { get; init; }

    public void Start()
    {
        _session.SendConsumerCredits(ConsumerId, -1);
    }

    public async ValueTask<ReceivedMessage> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        return await _reader.ReadAsync(cancellationToken);
    }

    public ValueTask AcknowledgeAsync(MessageDelivery messageDelivery, CancellationToken cancellationToken)
    {
        return _session.AcknowledgeAsync(messageDelivery, cancellationToken);
    }

    public ValueTask IndividualAcknowledgeAsync(in MessageDelivery messageDelivery, CancellationToken cancellationToken)
    {
        return _session.IndividualAcknowledgeAsync(messageDelivery, cancellationToken);
    }

    public void Acknowledge(in MessageDelivery messageDelivery)
    {
        _session.Acknowledge(messageDelivery);
    }

    public void IndividualAcknowledge(in MessageDelivery messageDelivery)
    {
        _session.IndividualAcknowledge(messageDelivery);
    }

    internal void OnMessage(ReceivedMessage message)
    {
        // TODO: What if try write is false?
        _ = _writer.TryWrite(message);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _session.CloseConsumer(ConsumerId);
    }    
}