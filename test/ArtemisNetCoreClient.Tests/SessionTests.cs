using ActiveMQ.Artemis.Core.Client.Framing;
using Xunit;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class SessionTests
{
    [Fact]
    public async Task should_establish_session()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();

        // Act
        var connectionFactory = new SessionFactory();
        var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);

        // Assert
        Assert.NotNull(session);
        await session.DisposeAsync();
    }

    [Theory]
    [InlineData(new[] { RoutingType.Anycast })]
    [InlineData(new[] { RoutingType.Multicast })]
    [InlineData(new[] { RoutingType.Anycast, RoutingType.Multicast })]
    public async Task should_create_address_with_selected_routing_type(RoutingType[] routingTypes)
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();
        
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);

        // Act
        var addressName = $"{Guid.NewGuid().ToString()}-{string.Join("-", routingTypes)}";
        await session.CreateAddress(addressName, routingTypes, testFixture.CancellationToken);
        
        // Assert
        var addressInfo = await session.GetAddressInfo(addressName, testFixture.CancellationToken);
        Assert.NotNull(addressInfo);
        Assert.Equal(addressName, addressInfo.Name);
        Assert.Equal(routingTypes, addressInfo.RoutingTypes);
    }

    [Theory]
    [InlineData(RoutingType.Anycast)]
    [InlineData(RoutingType.Multicast)]
    public async Task should_create_queue_with_selected_routing_type(RoutingType routingType)
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();
        
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, [routingType], testFixture.CancellationToken);
        
        // Act
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueue(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = routingType
        }, testFixture.CancellationToken);
        
        // Assert
        var queueInfo = await session.GetQueueInfo(queueName, testFixture.CancellationToken);
        Assert.NotNull(queueInfo);
        Assert.Equal(queueName, queueInfo.QueueName);
        Assert.Equal(addressName, queueInfo.AddressName);
        Assert.Equal(routingType, queueInfo.RoutingType);
    }

    [Fact]
    public async Task should_not_return_address_info_when_address_does_not_exist()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();

        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);

        // Act
        var addressName = Guid.NewGuid().ToString();
        var addressInfo = await session.GetAddressInfo(addressName, testFixture.CancellationToken);
        
        // Assert
        Assert.Null(addressInfo);
    }

    [Fact]
    public async Task should_not_return_queue_info_when_queue_does_not_exist()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();
        
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);

        // Act
        var queueName = Guid.NewGuid().ToString();
        var queueInfo = await session.GetQueueInfo(queueName, testFixture.CancellationToken);
        
        // Assert
        Assert.Null(queueInfo);
    }

    [Fact]
    public async Task should_create_and_dispose_consumer()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();
        
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);
        
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new [] { RoutingType.Multicast }, testFixture.CancellationToken);
        
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueue(new QueueConfiguration
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
    public async Task should_create_and_dispose_producer()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync();
        
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(testFixture.GetEndpoint(), testFixture.CancellationToken);
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new [] { RoutingType.Multicast }, testFixture.CancellationToken);
        
        // Act
        var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, testFixture.CancellationToken);

        await producer.DisposeAsync();
    }
}