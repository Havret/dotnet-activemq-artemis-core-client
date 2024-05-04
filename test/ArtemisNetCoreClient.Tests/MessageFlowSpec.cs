using System.Diagnostics.CodeAnalysis;
using ActiveMQ.Artemis.Core.Client.Framing;
using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class MessageFlowSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task Should_receive_messages_in_the_same_order_as_they_were_sent()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();

        var addressName = await testFixture.CreateAddressAsync();
        var queueName = await testFixture.CreateQueueAsync(addressName);
        const int numberOfMessages = 100;
        
        // Act
        var consumedMessagesTask = Task.Run(async () =>
        {
            await using var session = await connection.CreateSessionAsync();
            await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
            {
                QueueName = queueName,
            }, testFixture.CancellationToken);

            var messages = new List<Message>();
            for (int i = 0; i < numberOfMessages; i++)
            {
                messages.Add(await consumer.ReceiveMessageAsync(testFixture.CancellationToken));
            }

            return messages;
        });

        var sendMessagesTask = Task.Run(async () =>
        {
            await using var session = await connection.CreateSessionAsync();
            await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
            {
                Address = addressName
            }, testFixture.CancellationToken);

            var tasks = new List<ValueTask>(numberOfMessages);
            for (int i = 0; i < numberOfMessages; i++)
            {
                tasks.Add(producer.SendMessageAsync(new Message
                {
                    Headers = new Headers
                    {
                        Address = addressName,
                    },
                    Properties = new Dictionary<string, object?>
                    {
                        ["index"] = i
                    }
                }, testFixture.CancellationToken));
            }

            await Task.WhenAll(tasks.Select(t => t.AsTask()));
        });

        var messages = await consumedMessagesTask.WaitAsync(testFixture.CancellationToken);
        await sendMessagesTask.WaitAsync(testFixture.CancellationToken);
        
        // Assert
        Assert.Equal(numberOfMessages, messages.Count);
        for (int i = 0; i < numberOfMessages; i++)
        {
            Assert.Equal(i, messages[i].Properties["index"]);
        }
    }
}