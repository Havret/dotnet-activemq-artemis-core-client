namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Provides extension methods for the <see cref="IConnection"/> interface.
/// </summary>
public static class ConnectionExtensions
{
    /// <summary>
    /// Creates a session with the default configuration.
    /// </summary>
    public static Task<ISession> CreateSessionAsync(this IConnection connection, CancellationToken cancellationToken = default)
    {
        var configuration = new SessionConfiguration();
        return connection.CreateSessionAsync(configuration, cancellationToken);
    }
}