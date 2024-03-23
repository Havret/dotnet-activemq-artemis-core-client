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
}