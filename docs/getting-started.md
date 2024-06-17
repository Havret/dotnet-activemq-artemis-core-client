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

## Creating a Connection

To send or receive messages, you first need to establish a connection with the broker endpoint. This connection process starts by setting up an `Endpoint` class instance, which specifies the connection parameters.

### Initializing the Endpoint

You can initialize an `Endpoint` object using the following object initializer in C#:

```csharp
var endpoint = new Endpoint
{
    Host = "localhost",
    Port = 61616,
    User = "guest",
    Password = "guest"
};
```

- `Host` and `Port` properties specify the TCP endpoint for the connection.
- `User` and `Password` are credentials used to authenticate the client application with the broker.

### Establishing a Connection

To connect to an ActiveMQ Artemis node, create a `ConnectionFactory` object. This factory provides a method to asynchronously create connections using the previously defined `Endpoint` object.

Here is how you can connect to an ActiveMQ Artemis node using the `Endpoint` configuration:

```csharp
var connectionFactory = new ConnectionFactory();
var connection = await connectionFactory.CreateAsync(endpoint);
```

This example demonstrates how to connect to an ActiveMQ Artemis node using `localhost` on port `61616` with the username and password both set to `guest`.
