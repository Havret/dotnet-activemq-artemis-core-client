using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class MessagePropertiesSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_send_and_receive_message_with_properties()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync();
        
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, new [] { RoutingType.Multicast }, testFixture.CancellationToken);
        await using var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, testFixture.CancellationToken);
        // create queue
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueueAsync(new QueueConfiguration
        {
            Address = addressName,
            RoutingType = RoutingType.Multicast,
            Name = queueName
        }, testFixture.CancellationToken);
        
        await using var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName,
        }, testFixture.CancellationToken);
        
        // Act
        await producer.SendMessageAsync(new Message
        {
            Body = "test_payload"u8.ToArray(),
            Address = addressName,
            Properties = new Dictionary<string, object?>
            {
                ["null_property"] = null,
                ["bool_property_true"] = true,
                ["bool_property_false"] = false,
                ["byte_property"] = (byte)42,
                ["bytes_property"] = new byte[] { 1, 2, 3 },
                ["short_property"] = (short)42,
                ["int_property"] = 43,
                ["long_property"] = 44L,
                ["float_property"] = 45.1F,
                ["double_property"] = 46.2D,
                ["string_property"] = "string_value",
                ["char_property"] = 'c',
            }
        }, testFixture.CancellationToken);
        

        var receivedMessage = await consumer.ReceiveMessageAsync(testFixture.CancellationToken);
        
        // Assert
        Assert.NotNull(receivedMessage);
        Assert.Equal(12, receivedMessage.Properties.Count);
        Assert.Null(receivedMessage.Properties["null_property"]);
        Assert.True((bool)receivedMessage.Properties["bool_property_true"]!);
        Assert.False((bool)receivedMessage.Properties["bool_property_false"]!);
        Assert.Equal((byte)42, (byte)receivedMessage.Properties["byte_property"]!);
        Assert.Equal([1, 2, 3], (byte[])receivedMessage.Properties["bytes_property"]!);
        Assert.Equal((short)42, (short)receivedMessage.Properties["short_property"]!);
        Assert.Equal(43, (int)receivedMessage.Properties["int_property"]!);
        Assert.Equal(44L, (long)receivedMessage.Properties["long_property"]!);
        Assert.Equal(45.1F, (float)receivedMessage.Properties["float_property"]!);
        Assert.Equal(46.2D, (double)receivedMessage.Properties["double_property"]!);
        Assert.Equal("string_value", (string)receivedMessage.Properties["string_property"]!);
        Assert.Equal('c', (char)receivedMessage.Properties["char_property"]!);
    }
}