using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ActiveMQ.Artemis.Core.Client;

public class ConnectionFactory
{
    public async Task<IConnection> CreateAsync(Endpoint endpoint, CancellationToken cancellationToken = default)
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

        var transport = new Transport2(LoggerFactory.CreateLogger<Transport2>(), socket);


        return new Connection(LoggerFactory, transport);
    }
    
    public ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();
}