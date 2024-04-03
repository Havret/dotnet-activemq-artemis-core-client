using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ProducerTests
{
    [Test]
    public async Task should_send_message()
    {
        // Arrange
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new [] { RoutingType.Multicast }, default);
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, default);
        
        // Act
        await producer.SendMessage(new Message
        {
            Address = addressName,
            Durable = true,
            Body = "test_payload"u8.ToArray()
        }, default);


        await producer.DisposeAsync();
    }
}