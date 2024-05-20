# .NET Client for Apache ActiveMQ Artemis

[![Build](https://github.com/Havret/dotnet-activemq-artemis-core-client/actions/workflows/build.yml/badge.svg)](https://github.com/Havret/dotnet-activemq-artemis-core-client/actions/workflows/build.yml)

---

<div align="center">

  <img src="./readme/artemis.png" alt="ArtemisNetCoreClient" width="400"/>

</div>

---

Apache ActiveMQ Artemis is an open-source project to build a multi-protocol, embeddable, very high performance, clustered, asynchronous messaging system.

This .NET client library is an open-source effort to equip .NET developers with a powerful, straightforward client for Apache ActiveMQ Artemis. Utilizing the broker's Core protocol, this library focuses on high-performance messaging, ensuring compatibility and comprehensive feature support with Apache ActiveMQ Artemis.

## Running the tests

To run the tests, you need an Apache ActiveMQ Artemis server. The server can be hosted in a Docker container.

### Setting Up the Necessary Infrastructure

Ensure that Docker and Docker Compose are properly installed and configured on your machine.

1. Navigate to the `/test/artemis` directory.
2. Run the following command to spin up the broker:

```sh
docker-compose up -V -d
```

With the broker up and running, you can execute the test suite using the following command:

```sh
dotnet test
```

## Disclaimer

Please note that this project is currently under active development and is not considered production-ready. We are continuously working to improve and stabilize its features, but it does not yet meet all the requirements for production use.

If you are in search of a production-ready ActiveMQ Artemis client for .NET, we recommend checking out the [AMQP-based client](https://github.com/Havret/dotnet-activemq-artemis-client). This alternative client has been battle-tested in production environments for the last few years and supports a wide range of ActiveMQ Artemis features.

## License

This project is licensed under the [Apache-2.0 License](https://github.com/Havret/dotnet-activemq-artemis-core-client/blob/main/LICENSE). You are welcome to use it freely, without any restrictive obligations.
