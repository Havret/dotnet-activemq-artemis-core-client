using ActiveMQ.Artemis.Core.Client.Exceptions;
using ActiveMQ.Artemis.Core.Client.Framing;
using ActiveMQ.Artemis.Core.Client.InternalUtilities;
using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class SessionSpec(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData(new[] { RoutingType.Anycast })]
    [InlineData(new[] { RoutingType.Multicast })]
    [InlineData(new[] { RoutingType.Anycast, RoutingType.Multicast })]
    public async Task Should_create_address_with_selected_routing_type(RoutingType[] routingTypes)
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync();

        // Act
        var addressName = $"{Guid.NewGuid().ToString()}-{string.Join("-", routingTypes)}";
        await session.CreateAddressAsync(addressName, routingTypes, testFixture.CancellationToken);
        
        // Assert
        var addressInfo = await session.GetAddressInfoAsync(addressName, testFixture.CancellationToken);
        Assert.NotNull(addressInfo);
        Assert.Equal(routingTypes, addressInfo.RoutingTypes);
        Assert.Empty(addressInfo.QueueNames);
    }

    [Theory]
    [InlineData(RoutingType.Anycast)]
    [InlineData(RoutingType.Multicast)]
    public async Task Should_create_queue_with_selected_routing_type(RoutingType routingType)
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);

        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, [routingType], testFixture.CancellationToken);
        
        // Act
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueueAsync(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = routingType
        }, testFixture.CancellationToken);
        
        // Assert
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(queueName, queueInfo.QueueName);
        Assert.Equal(addressName, queueInfo.AddressName);
        Assert.Equal(routingType, queueInfo.RoutingType);
    }

    [Fact]
    public async Task Should_not_return_address_info_when_address_does_not_exist()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);

        // Act
        var addressName = Guid.NewGuid().ToString();
        var addressInfo = await session.GetAddressInfoAsync(addressName, testFixture.CancellationToken);
        
        // Assert
        Assert.Null(addressInfo);
    }

    [Fact]
    public async Task should_not_return_queue_info_when_queue_does_not_exist()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);

        // Act
        var queueName = Guid.NewGuid().ToString();
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        
        // Assert
        Assert.Null(queueInfo);
    }

    [Fact]
    public async Task Should_create_and_dispose_consumer()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);
        
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, [RoutingType.Multicast], testFixture.CancellationToken);
        
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueueAsync(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = RoutingType.Multicast
        }, testFixture.CancellationToken);
        
        // Act
        var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName
        }, testFixture.CancellationToken);

        await consumer.DisposeAsync();
    }
    
    [Fact]
    public async Task Should_create_and_dispose_producer()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);
        
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, new [] { RoutingType.Multicast }, testFixture.CancellationToken);
        
        // Act
        var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, testFixture.CancellationToken);

        await producer.DisposeAsync();
    }

    [Fact]
    public async Task Should_delete_queue()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);

        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddressAsync(addressName, new [] { RoutingType.Multicast }, testFixture.CancellationToken);
        
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueueAsync(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = RoutingType.Multicast
        }, testFixture.CancellationToken);
        
        // Act
        await session.DeleteQueueAsync(queueName, testFixture.CancellationToken);
        
        // Assert
        var queueInfo = await session.GetQueueInfoAsync(queueName, testFixture.CancellationToken);
        Assert.Null(queueInfo);
    }
    
    [Fact]
    public async Task Should_not_delete_queue_when_it_does_not_exist()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();
        await using var session = await connection.CreateSessionAsync(testFixture.CancellationToken);

        // Act & Assert
        var queueName = Guid.NewGuid().ToString();
        await Assert.ThrowsAsync<ActiveMQNonExistentQueueException>(() => session.DeleteQueueAsync(queueName, testFixture.CancellationToken));
    }
}