using ActiveMQ.Artemis.Core.Client.Framing;
using Xunit;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ConsumerTests
{
    [Fact]
    public async Task should_receive_message()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();

        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new[] { RoutingType.Anycast }, testFixture.CancellationToken);

        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueue(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = RoutingType.Anycast,
        }, testFixture.CancellationToken);
        
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, testFixture.CancellationToken);
        
        await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName,
        }, testFixture.CancellationToken);
        
        await producer.SendMessage(new Message
        {
            Address = addressName,
            Durable = true,
            Body = "test_payload"u8.ToArray()
        }, testFixture.CancellationToken);

        // Act
        var message = await consumer.ReceiveAsync(testFixture.CancellationToken);
        
        // Assert
        Assert.Equal("test_payload"u8.ToArray(), message.Body);
    }
}