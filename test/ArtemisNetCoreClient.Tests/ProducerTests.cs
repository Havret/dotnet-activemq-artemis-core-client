using ActiveMQ.Artemis.Core.Client.Framing;
using Xunit;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ProducerTests
{
    [Fact]
    public async Task should_send_message()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();
        
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new [] { RoutingType.Multicast }, testFixture.CancellationToken);
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, testFixture.CancellationToken);
        
        // Act
        await producer.SendMessage(new Message
        {
            Address = addressName,
            Durable = true,
            Body = "test_payload"u8.ToArray()
        }, testFixture.CancellationToken);
    }
}