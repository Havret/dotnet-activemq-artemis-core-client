using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class MessageTimestampSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_send_and_receive_message_with_timestamp_header()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync();
        
        var addressName = await testFixture.CreateAddressAsync();
        var queueName = await testFixture.CreateQueueAsync(addressName);
        
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, testFixture.CancellationToken);
        
        await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName,
        }, testFixture.CancellationToken);
        
        // Act
        var timestamp = DateTimeOffset.UtcNow;
        await producer.SendMessageAsync(new Message
        {
            Body = "test_msg"u8.ToArray(),
            Address = addressName,
            Timestamp = timestamp,
        }, testFixture.CancellationToken);

        var receivedMessage = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Assert
        Assert.NotNull(receivedMessage);
        Assert.Equal(timestamp.DropTicsPrecision(), receivedMessage.Headers.Timestamp);
    }
}