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
    }
}