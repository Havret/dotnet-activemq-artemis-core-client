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
        
        var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
        var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
        
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
    
    [Fact]
    public async Task should_individually_acknowledge_message()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        
        var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
        var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
        
        // Create a session with AutoCommitAcks enabled (default)
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);
        
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
            Body = "msg_1"u8.ToArray()
        }, testFixture.CancellationToken);
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "msg_2"u8.ToArray()
        }, testFixture.CancellationToken);

        // Receive the messages
        var receivedMessage1 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        var receivedMessage2 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Confirm message count before acknowledgment
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(2, queueInfo.MessageCount);

        // Acknowledge the first message
        await consumer.IndividualAcknowledgeAsync(receivedMessage1.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that one outstanding message remains on the queue
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(1, queueInfo.MessageCount);
        
        // Acknowledge the second message
        await consumer.IndividualAcknowledgeAsync(receivedMessage2.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that there are no outstanding messages on the queue
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(0, queueInfo.MessageCount);
    }

    [Fact]
    public async Task should_acknowledge_messages()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        
        var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
        var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
        
        // Create a session with AutoCommitAcks enabled (default)
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);
        
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
            Body = "msg_1"u8.ToArray()
        }, testFixture.CancellationToken);
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "msg_2"u8.ToArray()
        }, testFixture.CancellationToken);
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "msg_3"u8.ToArray()
        }, testFixture.CancellationToken);

        // Receive the messages
        _ = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        var receivedMessage2 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        var receivedMessage3 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Verify that there are three outstanding messages on the queue
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(3, queueInfo.MessageCount);

        // Acknowledge messages up to the second message (inclusive)
        await consumer.AcknowledgeAsync(receivedMessage2.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that one outstanding message remains on the queue (two messages were acknowledged in one go)
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(1, queueInfo.MessageCount);
        
        // Acknowledge messages up to the third (the last remaining) message (inclusive)
        await consumer.AcknowledgeAsync(receivedMessage3.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that there are no outstanding messages on the queue
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(0, queueInfo.MessageCount);
    }
    
    [Fact]
    public async Task should_acknowledge_individual_messages_only_post_session_commit_when_AutoCommitAcks_is_disabled()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        
        var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
        var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
        
        // Create a session with AutoCommitAcks disabled
        await using var session = await connection.CreateSessionAsync(new SessionConfiguration
        {
            AutoCommitAcks = false
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
            Body = "msg_1"u8.ToArray()
        }, testFixture.CancellationToken);
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "msg_2"u8.ToArray()
        }, testFixture.CancellationToken);

        // Receive the messages but do not commit the session yet
        var receivedMessage1 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        var receivedMessage2 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Confirm message count before acknowledgment
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(2, queueInfo.MessageCount);

        // Acknowledge messages individually
        await consumer.IndividualAcknowledgeAsync(receivedMessage1.MessageDelivery, testFixture.CancellationToken);
        await consumer.IndividualAcknowledgeAsync(receivedMessage2.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that the messages are still present on the queue as the session is not committed
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(2, queueInfo.MessageCount);
        
        // Commit the session and verify message clearance
        await session.CommitAsync(testFixture.CancellationToken);
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(0, queueInfo.MessageCount);
    }
    
    [Fact]
    public async Task should_acknowledge_all_messages_only_post_session_commit_when_AutoCommitAcks_is_disabled()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        
        var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
        var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
        
        // Create a session with AutoCommitAcks disabled
        await using var session = await connection.CreateSessionAsync(new SessionConfiguration
        {
            AutoCommitAcks = false
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
            Body = "msg_1"u8.ToArray()
        }, testFixture.CancellationToken);
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "msg_2"u8.ToArray()
        }, testFixture.CancellationToken);
        
        await producer.SendMessageAsync(new Message
        {
            Headers = new Headers
            {
                Address = addressName,
            },
            Body = "msg_3"u8.ToArray()
        }, testFixture.CancellationToken);

        // Receive the messages but do not commit the session yet
        _ = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        var receivedMessage2 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        var receivedMessage3 = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Confirm message count before acknowledgment
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(3, queueInfo.MessageCount);

        // Acknowledge messages up to the second message (inclusive)
        await consumer.AcknowledgeAsync(receivedMessage2.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that the messages are still present on the queue as the session is not committed
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(3, queueInfo.MessageCount);
        
        // Acknowledge messages up to the third message (inclusive)
        await consumer.AcknowledgeAsync(receivedMessage3.MessageDelivery, testFixture.CancellationToken);
        
        // Verify that the messages are still present on the queue as the session is not committed
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(3, queueInfo.MessageCount);
        
        // Commit the session and verify message clearance
        await session.CommitAsync(testFixture.CancellationToken);
        queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(0, queueInfo.MessageCount);
    }
}