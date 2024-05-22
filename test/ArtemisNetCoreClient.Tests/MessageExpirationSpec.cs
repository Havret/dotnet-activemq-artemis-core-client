using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class MessageExpirationSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_send_and_receive_message_with_expiration_header()
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
        var expiration = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(10));
        await producer.SendMessageAsync(new Message
        {
            Expiration = expiration,
            Durable = true,
            Body = "expiry_message"u8.ToArray(),
        }, testFixture.CancellationToken);

        var receivedMessage = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Assert
        Assert.NotNull(receivedMessage);
        Assert.Equal("expiry_message"u8.ToArray(), receivedMessage.Body.ToArray());
        Assert.Equal(expiration.DropTicsPrecision(), receivedMessage.Expiration);
    }

    [Fact]
    public async Task Should_deliver_expired_message_to_expiry_queue()
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
            QueueName = "ExpiryQueue",
        }, testFixture.CancellationToken);
        
        // Act
        var expiration = DateTimeOffset.UtcNow.Add(TimeSpan.FromMilliseconds(20));
        await producer.SendMessageAsync(new Message
        {
            Body = "expiry_message"u8.ToArray(),
            Expiration = expiration,
        }, testFixture.CancellationToken);
        
        var receivedMessage = await RetryUtil.RetryUntil(
            func: async () => await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
            until: msg => msg.Properties.TryGetValue("_AMQ_ORIG_ADDRESS", out var val)
                          && val is string origAddress
                          && origAddress == addressName,
            testFixture.CancellationToken
        );
        
        // Assert
        Assert.NotNull(receivedMessage);
        var originalAddress = Assert.IsType<string>(receivedMessage.Properties["_AMQ_ORIG_ADDRESS"]);
        Assert.Equal(addressName, originalAddress);
        
        var originalQueue = Assert.IsType<string>(receivedMessage.Properties["_AMQ_ORIG_QUEUE"]);
        Assert.Equal(queueName, originalQueue);
        
        var actualExpiry = Assert.IsType<long>(receivedMessage.Properties["_AMQ_ACTUAL_EXPIRY"]);
        Assert.InRange(actualExpiry, expiration.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        
        var originalMessageId = Assert.IsType<long>(receivedMessage.Properties["_AMQ_ORIG_MESSAGE_ID"]);
        Assert.NotEqual(0, originalMessageId);
    }
}