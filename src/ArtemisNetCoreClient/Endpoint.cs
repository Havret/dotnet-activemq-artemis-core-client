namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Defines the ActiveMQ Artemis endpoint.
/// </summary>
public class Endpoint
{
    /// <summary>
    /// Gets or sets the protocol scheme.
    /// </summary>
    public string Host { get; init; }

    /// <summary>
    /// Gets or sets the port number of the endpoint.
    /// </summary>
    public int Port { get; init; }
    
    /// <summary>
    /// Gets or sets the user name that is used for authentication.
    /// </summary>
    public string? User { get; init; }
    
    /// <summary>
    /// Gets or sets the password that is used for authentication.
    /// </summary>
    public string? Password { get; init; }
}