namespace ActiveMQ.Artemis.Core.Client;

/// <summary>
/// Provides configuration options for creating sessions in an ActiveMQ Artemis client.
/// </summary>
public class SessionConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the session should automatically commit the transaction
    /// after each message acknowledgment. When set to true, each time a message is acknowledged by a consumer
    /// created from this session, the session will automatically commit the transaction associated with that
    /// acknowledgment. This property simplifies transaction management but may affect performance and consistency
    /// depending on the use case.
    /// 
    /// Default value is true, which enables automatic commits. Set this to false if you need more control
    /// over when transactions are committed, such as when batching multiple acknowledgments into a single transaction
    /// for efficiency or consistency reasons.
    /// </summary>
    public bool AutoCommitAcks { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the session should automatically commit the transaction
    /// after each message send. When set to true, each time a message is sent by a producer created from this
    /// session, the session will automatically commit the transaction associated with that send. This property
    /// simplifies transaction management but may affect performance and consistency depending on the use case.
    ///
    /// Default value is true, which enables automatic commits. Set this to false if you need more control
    /// over when transactions are committed, such as when batching multiple sends into a single transaction
    /// for efficiency or consistency reasons.
    /// </summary>
    public bool AutoCommitSends { get; set; } = true;
}