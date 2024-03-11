using System.Net;
using System.Net.Sockets;

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

        return new Session(socket);
    }
}
