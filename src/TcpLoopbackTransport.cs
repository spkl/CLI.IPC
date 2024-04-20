// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace spkl.CLI.IPC;

/// <summary>
/// Establishes a local connection using TCP on an IPv4 loopback.
/// </summary>
[DataContract]
public class TcpLoopbackTransport : ITransport
{
    /// <summary>
    /// Gets the used TCP port.
    /// </summary>
    [DataMember]
    public int Port { get; private set; }

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
    public virtual void AfterHostBind(EndPoint? usedEndPoint)
    {
        if (usedEndPoint is not IPEndPoint ipEndPoint)
        {
            throw new InvalidOperationException("Bound endpoint is null or not an IPEndPoint.");
        }

        this.Port = ipEndPoint.Port;
    }

    /// <inheritdoc/>
    public virtual void AfterHostShutdown()
    {
    }
}
