using ActiveMQ.Artemis.Core.Client.Framing;
using Xunit;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class SessionTests
{
    [Fact]
    public async Task should_establish_session()
    {
        // Arrange
        var connectionFactory = new SessionFactory();

        // Act
        var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });

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
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });

        // Act
        var addressName = $"{Guid.NewGuid().ToString()}-{string.Join("-", routingTypes)}";
        await session.CreateAddress(addressName, routingTypes, default);
        
        // Assert
        var addressInfo = await session.GetAddressInfo(addressName, default);
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
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, [routingType], default);
        
        // Act
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueue(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = routingType
        }, default);
        
        // Assert
        var queueInfo = await session.GetQueueInfo(queueName, default);
        Assert.NotNull(queueInfo);
        Assert.Equal(queueName, queueInfo.QueueName);
        Assert.Equal(addressName, queueInfo.AddressName);
        Assert.Equal(routingType, queueInfo.RoutingType);
    }

    [Fact]
    public async Task should_not_return_address_info_when_address_does_not_exist()
    {
        // Arrange
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });

        // Act
        var addressName = Guid.NewGuid().ToString();
        var addressInfo = await session.GetAddressInfo(addressName, default);
        
        // Assert
        Assert.Null(addressInfo);
    }

    [Fact]
    public async Task should_not_return_queue_info_when_queue_does_not_exist()
    {
        // Arrange
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });

        // Act
        var queueName = Guid.NewGuid().ToString();
        var queueInfo = await session.GetQueueInfo(queueName, default);
        
        // Assert
        Assert.Null(queueInfo);
    }

    [Fact]
    public async Task should_create_and_dispose_consumer()
    {
        // Arrange
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new [] { RoutingType.Multicast }, default);
        
        var queueName = Guid.NewGuid().ToString();
        await session.CreateQueue(new QueueConfiguration
        {
            Address = addressName,
            Name = queueName,
            RoutingType = RoutingType.Multicast
        }, default);
        
        // Act
        var consumer = await session.CreateConsumerAsync(new ConsumerConfiguration
        {
            QueueName = queueName
        }, default);

        await consumer.DisposeAsync();
    }
    
    [Fact]
    public async Task should_create_and_dispose_producer()
    {
        // Arrange
        var connectionFactory = new SessionFactory();
        await using var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });
        var addressName = Guid.NewGuid().ToString();
        await session.CreateAddress(addressName, new [] { RoutingType.Multicast }, default);
        
        // Act
        var producer = await session.CreateProducerAsync(new ProducerConfiguration
        {
            Address = addressName
        }, default);

        await producer.DisposeAsync();
    }
}