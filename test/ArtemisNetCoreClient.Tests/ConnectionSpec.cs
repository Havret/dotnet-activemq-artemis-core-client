using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ConnectionSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task should_create_session()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();

        // Act
        var session = await connection.CreateSessionAsync();

        // Assert
        Assert.NotNull(session);
        await session.DisposeAsync();
    }
}