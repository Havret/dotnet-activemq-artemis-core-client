using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using NScenario;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ProducerSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_send_message_immediately_when_AutoCommitSends_is_enabled()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));

        await using var connection = await testFixture.CreateConnectionAsync();

        var (addressName, queueName) = await scenario.Step("Create address and queue", async () =>
        {
            var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
            var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
            return (addressName, queueName);
        });
        
        await using var session = await scenario.Step("Create a session with AutoCommitSends enabled (default)", async () =>
        {
            return await connection.CreateSessionAsync(testFixture.CancellationToken);
        });
        
        await using var producer = await scenario.Step("Create message producer", async () =>
        {
            return await session.CreateProducerAsync(new ProducerConfiguration
            {
                Address = addressName,
            }, testFixture.CancellationToken);
        });
        
        await scenario.Step("Send a message", async () =>
        {
            await producer.SendMessageAsync(new Message
            {
                Body = "msg_1"u8.ToArray(),
                Address = addressName,
            }, testFixture.CancellationToken);
        });

        await scenario.Step("Confirm message count (one messages should be available on the queue)", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(1, queueInfo.MessageCount);
        });
    }

    [Fact]
    public async Task Should_send_messages_only_post_session_commit_when_AutoCommitSends_is_disabled()
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

        await using var session = await scenario.Step("Create a session with AutoCommitSends disabled", async () =>
        {
            return await connection.CreateSessionAsync(new SessionConfiguration
            {
                AutoCommitSends = false,
            }, testFixture.CancellationToken);
        });

        await using var producer = await scenario.Step("Create message producer", async () =>
        {
            return await session.CreateProducerAsync(new ProducerConfiguration
            {
                Address = addressName,
            }, testFixture.CancellationToken);
        });

        await scenario.Step("Send two messages", async () =>
        {
            await producer.SendMessageAsync(new Message
            {
                Body = "msg_1"u8.ToArray(),
                Address = addressName,
            }, testFixture.CancellationToken);
            await producer.SendMessageAsync(new Message
            {
                Body = "msg_2"u8.ToArray(),
                Address = addressName,
            }, testFixture.CancellationToken);
        });

        await scenario.Step("Confirm message count (no message should be available on the queue)", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(0, queueInfo.MessageCount);
        });

        await scenario.Step("Commit the session", async () =>
        {
            await session.CommitAsync(testFixture.CancellationToken);
        });

        await scenario.Step("Confirm message count (two messages should be available on the queue)", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(2, queueInfo.MessageCount);
        });
    }
}