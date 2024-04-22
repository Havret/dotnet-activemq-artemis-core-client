using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using ActiveMQ.Artemis.Core.Client.Tests.Utils.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ConnectionFactorySpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task should_create_connection()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        
        var connectionFactory = new ConnectionFactory
        {
            LoggerFactory = new XUnitLoggerFactory(testOutputHelper),
        };

        // Act
        await using var connection = await connectionFactory.CreateAsync(TestFixture.GetEndpoint(), testFixture.CancellationToken);

        // Assert
        Assert.NotNull(connection);
        Assert.IsType<Connection>(connection);
    }
}