using ActiveMQ.Artemis.Core.Client.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests;

public class ConnectionSpec(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_create_session()
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
    
    [Fact]
    public async Task Should_create_multiple_sessions()
    {
        // Arrange
        await using var testFixture = await TestFixture.CreateAsync(testOutputHelper);
        await using var connection = await testFixture.CreateConnectionAsync();

        // Act
        await using var session1 = await connection.CreateSessionAsync();
        await using var session2 = await connection.CreateSessionAsync();

        // Assert
        Assert.NotNull(session1);
        Assert.NotNull(session2);
    }
}