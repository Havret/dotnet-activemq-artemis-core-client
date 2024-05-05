using System.Threading.Channels;

namespace ActiveMQ.Artemis.Core.Client;

internal class Consumer : IConsumer
{
    private readonly Session _session;
    private readonly ChannelReader<Message> _reader;
    private readonly ChannelWriter<Message> _writer;

    public Consumer(Session session)
    {
        _session = session;
        
        // TODO: Change to Bounded based on consumer credit
        var channel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions
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

    public async ValueTask<Message> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        return await _reader.ReadAsync(cancellationToken);
    }

    internal void OnMessage(Message message)
    {
        // TODO: What if try write is false?
        _ = _writer.TryWrite(message);
    }
}