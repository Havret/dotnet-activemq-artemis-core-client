# Roadmap

This document tracks features and issues that still need to be implemented before this library can be considered production-ready. Feature gaps were identified by comparing against the ActiveMQ Artemis Java core client (`artemis-core-client` module).

## High Priority

### Consumer Flow Control

**File:** `src/ArtemisNetCoreClient/Consumer.cs:15`

The internal message channel is currently unbounded (`Channel.CreateUnbounded`). It should be bounded and sized according to the consumer credit window negotiated with the broker. Additionally, the case where `TryWrite` returns `false` (line 59) is silently ignored and needs proper handling.

---

### Exception Type Coverage

**File:** `src/ArtemisNetCoreClient/Exceptions/ActiveMQExceptionType.cs`

Only one exception type is currently defined (`QueueDoesNotExist = 100`). The Artemis broker returns many more error codes. The full set from the Java `ActiveMQExceptionType` enum includes (among others):

- `AddressExists`, `AddressDoesNotExist`, `AddressFull`, `DeleteAddress`
- `QueueExists`, `QueueDoesNotExist`
- `DuplicateIdRejected`
- `Disconnected`, `NotConnected`, `RemoteDisconnect`
- `IncompatibleClientServer`
- `SessionCreationRejected`
- `TransactionRolledBack`, `TransactionTimeout`, `TransactionOutcomeUnknown`
- `Timeout`
- `BrokerShutdown`
- `LargeMessageError`
- `InvalidFilterExpression`
- `InvalidTransientQueueUse`
- `PropertyConversionError`
- `ClusterSecurityFailure`
- `IllegalState`
- `PacketNotSupported`

---

### Consumer Creation Failure Handling

**File:** `src/ArtemisNetCoreClient/Session.cs:248`

When consumer creation fails, the method returns `null!` instead of throwing a descriptive exception. This needs to throw an appropriate `ActiveMQException`.

---

### SSL/TLS Support

**File:** `src/ArtemisNetCoreClient/Transport.cs`

Only plain TCP connections are supported. SSL/TLS is required for any non-local or production deployment. Needs a `SslEndpoint` (or similar) and wrapping the socket stream with `SslStream`. The Java client supports both JDK SSL and OpenSSL (Netty) with configurable cipher suites, protocols, trust/key stores, and SNI.

---

### Session Failure Listeners

No mechanism for the application to be notified when a session loses its connection to the broker. The Java client has `SessionFailureListener` with `connectionFailed(exception, failoverSucceeded)` and `beforeReconnect(exception)` callbacks. Without this, applications cannot react to connectivity problems.

---

### Scheduled Message Delivery

**File:** `src/ArtemisNetCoreClient/Message.cs`

The `HDR_SCHEDULED_DELIVERY_TIME` (`_AMQ_SCHED_DELIVERY`) header is not exposed in the .NET `Message` API. Adding a `ScheduledDeliveryTime` property (a nullable `DateTimeOffset`) would allow messages to be held by the broker and delivered at a future time — a heavily-used Artemis feature.

---

### Duplicate Detection

**File:** `src/ArtemisNetCoreClient/Message.cs`

The `HDR_DUPLICATE_DETECTION_ID` (`_AMQ_DUPL_ID`) header is not exposed. Without it, producers cannot participate in the broker's idempotent send feature, which prevents duplicate delivery in the event of a retry.

---

## Medium Priority

### Message Selectors / Filters

**File:** `src/ArtemisNetCoreClient/ConsumerConfiguration.cs`

No filter expression support when creating consumers. The Artemis CORE protocol supports SQL-92 style selectors on queue subscriptions. A `Filter` property needs to be added to `ConsumerConfiguration` and wired through to `SessionConsumerCreateMessage`.

---

### Failover / High Availability

**File:** `src/ArtemisNetCoreClient/Endpoint.cs`

Only a single broker endpoint is supported. Production deployments require connecting to a cluster or failing over to a backup broker. Needs multi-endpoint support in `ConnectionFactory` with automatic reconnect and failover logic. The Java `ServerLocator` supports configurable retry interval, exponential backoff, max retry interval, reconnect attempts, and failover attempts.

---

### Temporary Queues

No API exists for creating temporary queues (auto-deleted when the session closes). This is a standard messaging pattern used for request-reply correlation. Maps to `SessionCreateConsumerMessage` with `temporary = true` in `QueueConfiguration`.

---

### Shared (Transient) Queues

No equivalent of `ClientSession.createSharedQueue()`. Shared queues exist only while at least one consumer is attached and are not durable. Used for fan-out patterns without permanent queue configuration.

---

### Consumer Browse-Only Mode

**File:** `src/ArtemisNetCoreClient/ConsumerConfiguration.cs`

`BrowseOnly` is hardcoded to `false` in `SessionConsumerCreateMessage`. A `BrowseOnly` flag on `ConsumerConfiguration` would allow non-destructive inspection of queued messages — messages are delivered but not consumed.

---

### XA Transaction Support

XA transactions are hardcoded to `false`. Full XA support requires implementing the protocol messages `SessionXAStartMessage`, `SessionXAEndMessage`, `SessionXAPrepareMessage`, `SessionXACommitMessage`, `SessionXARollbackMessage`, `SessionXAForgetMessage`, and `SessionXAGetInDoubtXidsResponseMessage`, and exposing an `IXASession` or similar interface. This is required for integration with distributed transaction coordinators.

---

### Pre-Acknowledgement Mode

**File:** `src/ArtemisNetCoreClient/SessionConfiguration.cs`

`PreAcknowledge` is hardcoded to `false` in `CreateSessionMessage`. When enabled, the broker marks messages as acknowledged as soon as they are delivered to the client — useful for very high-throughput, at-most-once scenarios where the extra round-trip of an explicit ack is not acceptable.

---

### Async Message Handler (Push Consumer)

The current consumer is pull-based only (`ReceiveMessageAsync`). The Java `ClientConsumer.setMessageHandler(MessageHandler)` allows the broker to push messages to an async callback, which is more efficient for sustained throughput. The .NET equivalent would be a `SetMessageHandler(Func<ReceivedMessage, Task>)` or similar API.

---

### Send Acknowledgement Handler

**File:** `src/ArtemisNetCoreClient/Producer.cs`

There is no async callback equivalent to Java's `SendAcknowledgementHandler`. This per-producer callback fires when the broker confirms receipt of a sent message, enabling fire-and-forget sends with optional reliable confirmation. The wire message `SessionProducerCreditsMessage` is already parsed; what is missing is surfacing the callback to the application.

---

### Message Headers: CorrelationID and ReplyTo

**File:** `src/ArtemisNetCoreClient/Message.cs`

The Java `Message` interface exposes `getCorrelationID()` / `setCorrelationID()` and `getReplyTo()` / `setReplyTo()` as first-class fields. These are stored as internal properties in the wire format and are essential for request-reply patterns without resorting to user-defined properties.

---

### Consumer Priority

**File:** `src/ArtemisNetCoreClient/ConsumerConfiguration.cs`

Consumer priority is hardcoded to `0` in `SessionCreateConsumerMessage`. Exposing a `Priority` property on `ConsumerConfiguration` lets applications influence which consumer on a queue receives messages first.

---

### Advanced Queue Configuration Options

**File:** `src/ArtemisNetCoreClient/QueueConfiguration.cs`

Many options from the Java `QueueConfiguration` are not exposed. The following should be added:

| Property | Description |
| --- | --- |
| `MaxConsumers` | Maximum number of concurrent consumers (-1 = unlimited) |
| `Exclusive` | Only one consumer receives messages |
| `PurgeOnNoConsumers` | Purge messages when last consumer detaches |
| `LastValue` | Keep only the latest message per key |
| `LastValueKey` | Property name used as the LVQ key |
| `NonDestructive` | Consumers read without consuming (repeatable reads) |
| `AutoDelete` | Delete queue when it has been unused |
| `AutoDeleteDelay` | Milliseconds of inactivity before auto-deletion |
| `AutoDeleteMessageCount` | Only auto-delete when fewer than N messages remain |
| `RingSize` | Retain only the last N messages; older messages are dropped |
| `ConsumersBeforeDispatch` | Minimum number of consumers required before any dispatching begins |
| `DelayBeforeDispatch` | Milliseconds to wait before dispatching if `ConsumersBeforeDispatch` is not met |
| `GroupRebalance` | Reassign message groups when consumers change |
| `GroupRebalancePauseDispatch` | Pause dispatch during a group rebalance |
| `GroupBuckets` | Number of partitions for consistent group hashing |
| `GroupFirstKey` | Property name used to identify the "first" message in a group |
| `User` | Associate the queue with a specific broker user |
| `Enabled` | Enable or disable the queue without deleting it |
| `AutoCreateAddress` | Automatically create the binding address if it does not exist |

---

### Enhanced QueueQuery and AddressQuery Responses

**Files:** `src/ArtemisNetCoreClient/Framing/Incoming/SessionQueueQueryResponseMessage.cs`, `SessionBindingQueryResponseMessage.cs`

The response messages currently parse a limited subset of the fields the broker returns. The Java `QueueQuery` and `AddressQuery` interfaces expose the full set of queue/address properties (filter, consumer count, auto-create settings, dispatch settings, etc.). Parsing these fields unlocks richer introspection without additional round-trips.

---

### Message Priority as Enum

**Files:** `src/ArtemisNetCoreClient/Message.cs:35`, `src/ArtemisNetCoreClient/ReceivedMessage.cs`

The `Priority` property is currently a raw `byte`. It should be a typed enum (e.g. `MessagePriority`) with named values matching the Artemis priority levels (0–9).

---

### Message Group Sequence

**File:** `src/ArtemisNetCoreClient/Message.cs`

The `HDR_GROUP_SEQUENCE` (`_AMQ_GROUP_SEQ`) header is not exposed. The broker uses this to order messages within a group. Clients that need precise control over group ordering need to set this property.

---

### Session Metadata

No equivalent of Java's `ClientSession.addMetaData(key, value)` / `addUniqueMetaData(key, value)`. Session metadata is visible in broker management views and is useful for tagging sessions with application name, instance ID, or environment labels for observability.

---

## Low Priority

### Cluster Topology Awareness

No subscription to broker cluster topology updates (`SubscribeClusterTopologyUpdatesMessage`). In a clustered deployment the broker broadcasts its live/backup topology so clients can fail over without a hard-coded list. This is a prerequisite for full HA support.

---

### Connection Pooling

No built-in connection pool. Users are responsible for managing connection lifecycles themselves. A simple pool would improve usability for high-concurrency scenarios.

---

### Request-Reply Helpers

No built-in support for the request-reply messaging pattern. Needs helper methods that handle reply-to address setup, correlation ID generation, and response matching (similar to `ClientRequestor` in the Java client).

---

### Large Message Streaming

**File:** `src/ArtemisNetCoreClient/Framing/`

No support for the large message protocol (`SessionSendLargeMessage`, `SessionSendContinuationMessage`, `SessionReceiveLargeMessage`, `SessionReceiveContinuationMessage`). Messages larger than the configured threshold must be streamed in chunks. Without this, very large payloads either fail or cause excessive memory pressure.

---

### Producer and Consumer Rate Limiting

No equivalent of `ServerLocator.setProducerMaxRate()` / `setConsumerMaxRate()`. Rate limiting caps the throughput of a single producer or consumer, which is useful for protecting downstream systems.

---

### Connection Keep-Alive / Configurable TTL

No configurable `connectionTTL` or explicit `Ping` packet scheduling. The Java client sends periodic pings and the broker closes idle connections after `connectionTTL` milliseconds. Without this, stale connections may go undetected.

---

### Interceptor Framework

No incoming or outgoing packet interceptor support. The Java `ServerLocator` accepts `Interceptor` instances that can inspect or mutate every packet. This is used for debugging, metrics, and protocol extensions.

---

### Batch Send

No API for sending multiple messages in a single operation. Users must manage transactions manually to group sends. A `SendBatchAsync` or similar method would improve ergonomics.

---

### RoutingType String Caching

**File:** `src/ArtemisNetCoreClient/Framing/Outgoing/SessionSendMessage.cs:142`

The `MessageHeaders.RoutingType` string is re-encoded on every send. It can be pre-encoded and cached to reduce per-message allocations.

---

## Documentation

- Architecture overview (transport, framing, session multiplexing)
- Full error code reference for `ActiveMQExceptionType`
- Advanced usage patterns (transactions, grouping, HA)
- Large message handling guide
- Troubleshooting guide
