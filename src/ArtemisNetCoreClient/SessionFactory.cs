using System.Net;
using System.Net.Sockets;
using ActiveMQ.Artemis.Core.Client.Framing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ActiveMQ.Artemis.Core.Client;

public class SessionFactory
{
    public async Task<ISession> CreateAsync(Endpoint endpoint, CancellationToken cancellationToken = default)
    {
        var ipAddresses = IPAddress.TryParse(endpoint.Host, out var ip)
            ? [ip]
            : await Dns.GetHostAddressesAsync(endpoint.Host, cancellationToken).ConfigureAwait(false);
        
        Socket? socket = null;
        Exception? exception = null;
        
        foreach (var ipAddress in ipAddresses)
        {
            if ((ipAddress.AddressFamily == AddressFamily.InterNetwork && !Socket.OSSupportsIPv4) ||
                (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && !Socket.OSSupportsIPv6))
            {
                continue;
            }

            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(ipAddress, endpoint.Port, cancellationToken).ConfigureAwait(false);
                exception = null;
                break;
            }
            catch (Exception e)
            {
                exception = e;
                socket.Dispose();
                socket = null;
            }
        }
        
        if (socket == null)
        {
            throw exception ?? new SocketException((int)SocketError.AddressNotAvailable);
        }
        
        var createSessionMessageV2 = new CreateSessionMessageV2
        {
            Name = Guid.NewGuid().ToString(),
            SessionChannelId = 10,
            Version = 135,
            Username = endpoint.User,
            Password = endpoint.Password,
            MinLargeMessageSize = 100 * 1024,
            Xa = false,
            AutoCommitSends = true,
            AutoCommitAcks = true,
            PreAcknowledge = false,
            WindowSize = -1,
            DefaultAddress = null,
            ClientId = null,
        };

        var transport = new Transport(socket);

        await transport.SendAsync(createSessionMessageV2, 1, cancellationToken);

        var receivedPacket = await transport.ReceiveAsync(cancellationToken);

        if (receivedPacket is CreateSessionResponseMessage createSessionResponseMessage)
        {
            var session = new Session(transport, LoggerFactory)
            {
                ChannelId = createSessionMessageV2.SessionChannelId,
                ServerVersion = createSessionResponseMessage.ServerVersion
            };
            await session.StartAsync(cancellationToken);
            return session;
        }
        else
        {
            throw new InvalidOperationException("Received invalid response from the broker");
        }
    }
    
    public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();
}