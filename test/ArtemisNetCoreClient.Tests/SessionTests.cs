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

    [Test, Ignore("WIP")]
    public async Task should_create_address()
    {
        // Arrange
        var connectionFactory = new SessionFactory();
        var session = await connectionFactory.CreateAsync(new Endpoint
        {
            Host = "localhost",
            Port = 5445,
            User = "artemis",
            Password = "artemis"
        });
        
        // Act && Assert
        await session.CreateAddress("myaddress", Enumerable.Empty<RoutingType>(), false, default);
    }
}