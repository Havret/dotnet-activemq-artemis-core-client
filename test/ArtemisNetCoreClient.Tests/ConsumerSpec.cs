using ActiveMQ.Artemis.Core.Client.Framing;
using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;


public class ConsumerSpec(ITestOutputHelper testOutputHelper)
{
    [Fact] 
    public async Task should_receive_message()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);
        
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, new[] { RoutingType.Anycast }, testFixture.CancellationToken);

        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueueAsync(new QueueConfiguration
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
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "test_payload"u8.ToArray()
        }, testFixture.CancellationToken);

        // Act
        var message = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Assert
        Assert.Equal("test_payload"u8.ToArray(), message.Body.ToArray());
    }
}