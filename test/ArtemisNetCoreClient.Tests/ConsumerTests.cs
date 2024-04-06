using ActiveMQ.Artemis.Core.Client.Framing;
using Xunit;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ConsumerTests
{
    [Fact]
    public async Task should_receive_message()
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
        await session.CreateAddress(addressName, new[] { RoutingType.Anycast }, default);


        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueue(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = RoutingType.Anycast,
        }, default);
        
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, default);


        await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName,
        }, default);
        
        await producer.SendMessage(new Message
        {
            Address = addressName,
            Durable = true,
            Body = "test_payload"u8.ToArray()
        }, default);

        // Act
        var message = await consumer.ReceiveAsync(default);
        
        // Assert
        Assert.Equal("test_payload"u8.ToArray(), message.Body);
    }
}