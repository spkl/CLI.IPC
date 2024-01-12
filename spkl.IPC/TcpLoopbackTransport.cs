using System.Net;
using System.Net.Sockets;

namespace spkl.IPC;

/// <summary>
/// Establishes a local connection using TCP on an IPv4 loopback.
/// </summary>
public class TcpLoopbackTransport : ITransport
{
    public int Port { get; }

    public TcpLoopbackTransport(int port)
    {
        this.Port = port;
    }

    public Socket Socket => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public EndPoint EndPoint => new IPEndPoint(IPAddress.Loopback, this.Port);

    public virtual void BeforeHostStart()
    {
    }

    public virtual void AfterHostShutdown()
    {
    }
}
