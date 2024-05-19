using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using NScenario;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ConsumerSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_receive_message()
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
            Address = addressName,
            Body = "test_payload"u8.ToArray()
        }, testFixture.CancellationToken);

        // Act
        var message = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);

        // Assert
        Assert.Equal("test_payload"u8.ToArray(), message.Body.ToArray());
    }

    [Fact]
    public async Task Should_individually_acknowledge_message()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));

        await using var connection = await testFixture.CreateConnectionAsync();

        var (addressName, queueName) = await scenario.Step("Create address and queue", async () =>
        {
            var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
            var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
            return (addressName, queueName);
        });

        await using var session = await scenario.Step("Create a session with AutoCommitAcks enabled (default)",
            async () => { return await connection.CreateSessionAsync(testFixture.CancellationToken); });

        await scenario.Step("Send two messages", async () =>
        {
            await testFixture.SendMessageAsync(addressName, "msg_1"u8.ToArray());
            await testFixture.SendMessageAsync(addressName, "msg_2"u8.ToArray());
        });

        await using var consumer = await scenario.Step("Create message consumer", async () =>
        {
            return await session.CreateConsumerAsync(new ConsumerConfiguration
            {
                QueueName = queueName,
            }, testFixture.CancellationToken);
        });

        var messages = await scenario.Step("Receive the messages", async () =>
        {
            return new[]
            {
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken)
            };
        });

        await scenario.Step("Confirm message count before acknowledgment", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(2, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge the first message",
            async () => { await consumer.IndividualAcknowledgeAsync(messages[0].MessageDelivery, testFixture.CancellationToken); });

        await scenario.Step("Verify that one outstanding message remains on the queue", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(1, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge the second message",
            async () => { await consumer.IndividualAcknowledgeAsync(messages[1].MessageDelivery, testFixture.CancellationToken); });

        await scenario.Step("Verify that there are no outstanding messages on the queue", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(0, queueInfo.MessageCount);
        });
    }

    [Fact]
    public async Task Should_acknowledge_messages()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));

        await using var connection = await testFixture.CreateConnectionAsync();

        var (addressName, queueName) = await scenario.Step("Create address and queue", async () =>
        {
            var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
            var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
            return (addressName, queueName);
        });

        await using var session = await scenario.Step("Create a session with AutoCommitAcks enabled (default)",
            async () => { return await connection.CreateSessionAsync(testFixture.CancellationToken); });

        await scenario.Step("Send three messages", async () =>
        {
            await testFixture.SendMessageAsync(addressName, "msg_1"u8.ToArray());
            await testFixture.SendMessageAsync(addressName, "msg_2"u8.ToArray());
            await testFixture.SendMessageAsync(addressName, "msg_3"u8.ToArray());
        });

        await using var consumer = await scenario.Step("Create message consumer", async () =>
        {
            return await session.CreateConsumerAsync(new ConsumerConfiguration
            {
                QueueName = queueName,
            }, testFixture.CancellationToken);
        });

        var messages = await scenario.Step("Receive the messages", async () =>
        {
            return new[]
            {
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken)
            };
        });

        await scenario.Step("Confirm message count before acknowledgment", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(3, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge messages up to the second message",
            async () => { await consumer.AcknowledgeAsync(messages[1].MessageDelivery, testFixture.CancellationToken); });

        await scenario.Step("Verify that one outstanding message remains on the queue", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(1, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge the last remaining message",
            async () => { await consumer.AcknowledgeAsync(messages[2].MessageDelivery, testFixture.CancellationToken); });

        await scenario.Step("Verify that there are no outstanding messages on the queue", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(0, queueInfo.MessageCount);
        });
    }

    [Fact]
    public async Task Should_acknowledge_individual_messages_only_post_session_commit_when_AutoCommitAcks_is_disabled()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));

        await using var connection = await testFixture.CreateConnectionAsync();

        var (addressName, queueName) = await scenario.Step("Create address and queue", async () =>
        {
            var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
            var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
            return (addressName, queueName);
        });

        await using var session = await scenario.Step("Create a session with AutoCommitAcks disabled", async () =>
        {
            return await connection.CreateSessionAsync(new SessionConfiguration
            {
                AutoCommitAcks = false
            }, testFixture.CancellationToken);
        });

        await scenario.Step("Send two messages", async () =>
        {
            await testFixture.SendMessageAsync(addressName, "msg_1"u8.ToArray());
            await testFixture.SendMessageAsync(addressName, "msg_2"u8.ToArray());
        });

        await using var consumer = await scenario.Step("Create message consumer", async () =>
        {
            return await session.CreateConsumerAsync(new ConsumerConfiguration
            {
                QueueName = queueName,
            }, testFixture.CancellationToken);
        });

        var messages = await scenario.Step("Receive the messages but do not commit the session yet", async () =>
        {
            return new[]
            {
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken)
            };
        });

        await scenario.Step("Confirm message count before acknowledgment", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(2, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge messages individually", async () =>
        {
            await consumer.IndividualAcknowledgeAsync(messages[0].MessageDelivery, testFixture.CancellationToken);
            await consumer.IndividualAcknowledgeAsync(messages[1].MessageDelivery, testFixture.CancellationToken);
        });

        await scenario.Step("Verify that the messages are still present on the queue as the session is not committed", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(2, queueInfo.MessageCount);
        });

        await scenario.Step("Commit the session and verify message clearance", async () =>
        {
            await session.CommitAsync(testFixture.CancellationToken);
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(0, queueInfo.MessageCount);
        });
    }

    [Fact]
    public async Task Should_acknowledge_all_messages_only_post_session_commit_when_AutoCommitAcks_is_disabled()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));

        await using var connection = await testFixture.CreateConnectionAsync();

        var (addressName, queueName) = await scenario.Step("Create address and queue", async () =>
        {
            var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
            var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
            return (addressName, queueName);
        });

        await using var session = await scenario.Step("Create a session with AutoCommitAcks disabled", async () =>
        {
            return await connection.CreateSessionAsync(new SessionConfiguration
            {
                AutoCommitAcks = false
            }, testFixture.CancellationToken);
        });

        await scenario.Step("Send three messages", async () =>
        {
            await testFixture.SendMessageAsync(addressName, "msg_1"u8.ToArray());
            await testFixture.SendMessageAsync(addressName, "msg_2"u8.ToArray());
            await testFixture.SendMessageAsync(addressName, "msg_3"u8.ToArray());
        });

        await using var consumer = await scenario.Step("Create message consumer", async () =>
        {
            return await session.CreateConsumerAsync(new ConsumerConfiguration
            {
                QueueName = queueName,
            }, testFixture.CancellationToken);
        });

        var messages = await scenario.Step("Receive the messages but do not commit the session yet", async () =>
        {
            return new[]
            {
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                await consumer.ReceiveMessageAsync(testFixture.CancellationToken)
            };
        });

        await scenario.Step("Confirm message count before acknowledgment", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(3, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge messages up to the second message", async () =>
        {
            await consumer.AcknowledgeAsync(messages[1].MessageDelivery, testFixture.CancellationToken);
        });

        await scenario.Step("Verify that the messages are still present on the queue", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(3, queueInfo.MessageCount);
        });

        await scenario.Step("Acknowledge the third message", async () =>
        {
            await consumer.AcknowledgeAsync(messages[2].MessageDelivery, testFixture.CancellationToken);
        });

        await scenario.Step("Verify that the messages are still present on the queue", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(3, queueInfo.MessageCount);
        });

        await scenario.Step("Commit the session and verify message clearance", async () =>
        {
            await session.CommitAsync(testFixture.CancellationToken);
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(0, queueInfo.MessageCount);
        });
    }
}