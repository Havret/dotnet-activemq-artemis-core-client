using ActiveMQ.Artemis.Core.Client.InternalUtilities;
using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using NScenario;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class AnonymousProducerSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_send_message_immediately_with_AnonymousProducer_when_AutoCommitSends_is_enabled()
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
        
        await using var session = await scenario.Step("Create a session with AutoCommitSends enabled (default)", async () =>
        {
            return await connection.CreateSessionAsync(testFixture.CancellationToken);
        });
        
        await using var producer = await scenario.Step("Create Anonymous Producer", async () =>
        {
            return await session.CreateAnonymousProducerAsync(testFixture.CancellationToken);
        });

        await scenario.Step("Send a message", async () =>
        {
            await producer.SendMessageAsync(addressName, RoutingType.Anycast, new Message
            {
                Body = "msg_1"u8.ToArray(),
            }, testFixture.CancellationToken);
        });

        await scenario.Step("Confirm message count (one message should be available on the queue)", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(1, queueInfo.MessageCount);
        });
    }
    
    [Fact]
    public async Task Should_send_messages_only_post_session_commit_with_AnonymousProducer_when_AutoCommitSends_is_disabled()
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

        await using var producer = await scenario.Step("Create Anonymous Producer", async () =>
        {
            return await session.CreateAnonymousProducerAsync(testFixture.CancellationToken);
        });

        await scenario.Step("Send two messages", async () =>
        {
            await producer.SendMessageAsync(addressName, RoutingType.Anycast, new Message
            {
                Body = "msg_1"u8.ToArray(),
            }, testFixture.CancellationToken);
            await producer.SendMessageAsync(addressName, RoutingType.Anycast, new Message
            {
                Body = "msg_2"u8.ToArray(),
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

    [Fact]
    public async Task Should_send_messages_to_anycast_or_multicast_queues_depending_on_selected_routing_type()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));
        
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync();

        var address = await scenario.Step("Create address supporting both anycast and multicast routing types", async () =>
        {
            var address = Guid.NewGuid().ToString();
            await session.CreateAddressAsync(address, [RoutingType.Anycast, RoutingType.Multicast], testFixture.CancellationToken);
            return address;
        });
        
        var anycastQueue = await scenario.Step("Create an anycast queue", async () =>
        {
            var queueName = Guid.NewGuid().ToString();
            await session.CreateQueueAsync(new QueueConfiguration
            {
                Address = address,
                Name = queueName,
                RoutingType = RoutingType.Anycast,
            }, testFixture.CancellationToken);
            return queueName;
        });
        
        var multicastQueue = await scenario.Step("Create a multicast queue", async () =>
        {
            var queueName = Guid.NewGuid().ToString();
            await session.CreateQueueAsync(new QueueConfiguration
            {
                Address = address,
                Name = queueName,
                RoutingType = RoutingType.Multicast
            }, testFixture.CancellationToken);
            return queueName;
        });
        
        await using var producer = await scenario.Step("Create Anonymous Producer", async () =>
        {
            return await session.CreateAnonymousProducerAsync(testFixture.CancellationToken);
        });
        
        await scenario.Step("Send two messages to the anycast queue", async () =>
        {
            for (int i = 0; i < 2; i++)
            {
                await producer.SendMessageAsync(address, RoutingType.Anycast, new Message
                {
                    Body = "anycast_msg"u8.ToArray(),
                }, testFixture.CancellationToken);
            }
        });
        
        await scenario.Step("Confirm message count (two messages should be available on the anycast queue)", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(anycastQueue, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(2, queueInfo.MessageCount);

            await scenario.Step("Verify message payload", async () =>
            {
                await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
                {
                    QueueName = anycastQueue
                }, testFixture.CancellationToken);
                var messages = new[]
                {
                    await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                    await consumer.ReceiveMessageAsync(testFixture.CancellationToken)
                };
                Assert.All(messages, message =>
                {
                    Assert.NotNull(message);
                    Assert.Equal("anycast_msg"u8.ToArray(), message.Body.ToArray());
                });
            });
        });
        
        await scenario.Step("Send two messages to the multicast queue", async () =>
        {
            for (int i = 0; i < 2; i++)
            {
                await producer.SendMessageAsync(address, RoutingType.Multicast, new Message
                {
                    Durable = true,
                    Body = "multicast_msg"u8.ToArray(),
                }, testFixture.CancellationToken);
            }
        });
        
        await scenario.Step("Confirm message count (two messages should be available on the multicast queue)", async () =>
        {
            var queueInfo = await session.GetQueueInfoAsync(multicastQueue, testFixture.CancellationToken);
            Assert.NotNull(queueInfo);
            Assert.Equal(2, queueInfo.MessageCount);
            
            await scenario.Step("Verify message payload", async () =>
            {
                await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
                {
                    QueueName = multicastQueue
                }, testFixture.CancellationToken);
                var messages = new[]
                {
                    await consumer.ReceiveMessageAsync(testFixture.CancellationToken),
                    await consumer.ReceiveMessageAsync(testFixture.CancellationToken)
                };
                Assert.All(messages, message =>
                {
                    Assert.NotNull(message);
                    Assert.Equal("multicast_msg"u8.ToArray(), message.Body.ToArray());
                });
            });
        });
    }
    
    [Fact]
    public async Task Should_send_message_to_specific_address_in_a_fire_and_forget_manner()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        var scenario = TestScenarioFactory.Default(new XUnitOutputAdapter(testOutputHelper));
        
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync();

        var (addressName, queueName) = await scenario.Step("Create address and queue", async () =>
        {
            var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
            var queueName = await testFixture.CreateQueueAsync(addressName, RoutingType.Anycast);
            return (addressName, queueName);
        });
        
        await scenario.Step("Send a message in a fire-and-forget manner", async () =>
        {
            await using var producer = await session.CreateAnonymousProducerAsync(testFixture.CancellationToken);
            
            // ReSharper disable once MethodHasAsyncOverload
            producer.SendMessage(address: addressName, routingType: RoutingType.Anycast, message: new Message
            {
                Body = "fire_and_forget_msg"u8.ToArray(),
            });
        });
        
        await scenario.Step("Confirm message count (one message should be available on the queue)", async () =>
        {
            await RetryUtil.RetryUntil(
                func: () => session.GetQueueInfoAsync(queueName, testFixture.CancellationToken),
                until: info => info?.MessageCount == 1,
                cancellationToken: testFixture.CancellationToken
            );
        });
        
        await scenario.Step("Verify message payload", async () =>
        {
            await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
            {
                QueueName = queueName
            }, testFixture.CancellationToken);
            var message = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
            Assert.NotNull(message);
            Assert.Equal("fire_and_forget_msg"u8.ToArray(), message.Body.ToArray());
        });
    }    
}