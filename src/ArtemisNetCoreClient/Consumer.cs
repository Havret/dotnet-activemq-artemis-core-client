using System.Threading.Channels;
using ActiveMQ.Artemis.Core.Client.Framing;

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

        // TODO: should this really be fire and forget?
        _ = session.SendAsync(new SessionConsumerFlowCreditMessage
        {
            ConsumerId = ConsumerId,
            Credits = 100
        }, default);
    }

    public required long ConsumerId { get; init; }
    
    public async ValueTask DisposeAsync()
    {
        var request = new SessionConsumerCloseMessage
        {
            ConsumerId = ConsumerId
        };
        _ = await _session.SendBlockingAsync<SessionConsumerCloseMessage, NullResponse>(request, default);
    }

    public async ValueTask<Message> ReceiveAsync(CancellationToken cancellationToken)
    {
        return await _reader.ReadAsync(cancellationToken);
    }

    internal void OnMessage(Message message)
    {
        // TODO: What if try write is false?
        _ = _writer.TryWrite(message);
    }
}