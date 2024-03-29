using ActiveMQ.Artemis.Core.Client.Framing;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class SessionTests
{
    [Test]
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
        Assert.IsNotNull(session);
        await session.DisposeAsync();
    }

    [TestCase(new[] { RoutingType.Anycast })]
    [TestCase(new[] { RoutingType.Multicast })]
    [TestCase(new[] { RoutingType.Anycast, RoutingType.Multicast })]
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
        Assert.That(addressInfo.Name, Is.EqualTo(addressName));
        CollectionAssert.AreEqual(routingTypes, addressInfo.RoutingTypes);
    }

    [TestCase(RoutingType.Anycast)]
    [TestCase(RoutingType.Multicast)]
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
        Assert.That(queueInfo, Is.Not.Null);
        Assert.That(queueInfo?.QueueName, Is.EqualTo(queueName));
        Assert.That(queueInfo?.AddressName, Is.EqualTo(addressName));
        Assert.That(queueInfo?.RoutingType, Is.EqualTo(routingType));
    }

    [Test]
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
        Assert.That(addressInfo, Is.Null);
    }

    [Test]
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
        Assert.That(queueInfo, Is.Null);
    }

    [Test]
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
    
    [Test]
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
        var producer = await session.CreateProducerAsync(new ProducerConfiguration()
        {
            Address = addressName
        }, default);

        await producer.DisposeAsync();
    }
}