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
        
        session.SendConsumerCredits(ConsumerId, -1);
    }

    public required long ConsumerId { get; init; }

    public async ValueTask DisposeAsync()
    {
        await _session.CloseConsumer(ConsumerId);
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

    internal void OnMessage(ReceivedMessage message)
    {
        // TODO: What if try write is false?
        _ = _writer.TryWrite(message);
    }
}