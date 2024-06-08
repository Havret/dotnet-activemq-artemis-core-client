---
id: getting-started
title: Getting started
sidebar_label: Getting Started
---

ArtemisNetCoreClient is an open-source effort to equip .NET developers with a powerful, straightforward client for Apache ActiveMQ Artemis. Utilizing the broker's CORE protocol, this library focuses on high-performance messaging, ensuring compatibility and comprehensive feature support with Apache ActiveMQ Artemis.

## Installation

The library is distributed via [NuGet](https://www.nuget.org/packages/ArtemisNetCoreClient). You can add ArtemisNetCoreClient NuGet package using dotnet CLI:

```sh
dotnet add package  ArtemisNetCoreClient --prerelease
```

## API overview

The API interfaces and classes are defined in the `ActiveMQ.Artemis.Core.Client` namespace:

```csharp
using ActiveMQ.Artemis.Core.Client;
```

The main API interfaces and classes are:

- `IConnection`:  represents a connection with the broker
- `ConnectionFactory`:  constructs `IConnection` instances
- `ISession`: represents a context for producing and consuming messages
- `IConsumer`: represents a message consumer
- `IProducer`: represents a message producer attached to a specified *address*
- `IAnonymousProducer`: represents a message producer capable of sending messages to multiple addresses
