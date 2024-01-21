// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;

namespace spkl.CLI.IPC;

/// <summary>
/// Establishes a local connection using TCP on an IPv4 loopback.
/// </summary>
public class TcpLoopbackTransport : ITransport
{
    /// <summary>
    /// Gets the used TCP port.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// </summary>
    public TcpLoopbackTransport(int port)
    {
        this.Port = port;
    }

    /// <inheritdoc/>
    public Socket Socket => new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    /// <inheritdoc/>
    public EndPoint EndPoint => new IPEndPoint(IPAddress.Loopback, this.Port);

    /// <inheritdoc/>
    public virtual void BeforeHostStart()
    {
    }

    /// <inheritdoc/>
    public virtual void AfterHostShutdown()
    {
    }
}
