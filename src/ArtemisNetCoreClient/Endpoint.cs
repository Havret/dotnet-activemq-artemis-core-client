namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Defines the ActiveMQ Artemis endpoint.
/// </summary>
public class Endpoint
{
    /// <summary>
    /// Gets or sets the host of the endpoint.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Gets or sets the port number of the endpoint.
    /// </summary>
    public int Port { get; init; }
    
    /// <summary>
    /// Gets or sets the username that is used for authentication.
    /// </summary>
    public string? User { get; init; }
    
    /// <summary>
    /// Gets or sets the password that is used for authentication.
    /// </summary>
    public string? Password { get; init; }
}