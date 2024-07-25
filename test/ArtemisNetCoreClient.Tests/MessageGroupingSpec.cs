using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class MessageGroupingSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_deliver_messages_with_the_same_GroupId_to_the_same_consumer()
    {
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync();
        
        var addressName = await testFixture.CreateAddressAsync(RoutingType.Anycast);
        var queueName = await testFixture.CreateQueueAsync(addressName);
        
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        });

        await using var consumer1 = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName
        });
        await using var consumer2 = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName
        });
        await using var consumer3 = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName
        });

        await SendMessagesToGroup(producer, "group1", 5);
        await SendMessagesToGroup(producer, "group2", 5);
        await SendMessagesToGroup(producer, "group3", 5);

        await AssertReceivedAllMessagesWithTheSameGroupId(consumer1, 5);
        await AssertReceivedAllMessagesWithTheSameGroupId(consumer2, 5);
        await AssertReceivedAllMessagesWithTheSameGroupId(consumer3, 5);
    }
    
    private static async Task SendMessagesToGroup(IProducer producer, string groupId, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            await producer.SendMessageAsync(new Message
            {
                GroupId = groupId,
            });
        }
    }
    
    private async Task AssertReceivedAllMessagesWithTheSameGroupId(IConsumer consumer, int count)
    {
        var messages = new List<ReceivedMessage>();
        for (int i = 1; i <= count; i++)
        {
            var message = await consumer.ReceiveMessageAsync();
            messages.Add(message);
        }

        Assert.Single(messages.GroupBy(x => x.GroupId));
    }    
}