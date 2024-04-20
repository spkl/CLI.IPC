// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;

namespace spkl.CLI.IPC;

/// <summary>
/// Controls the specifics of how the connection between host and client is established (i.e. protocol and address).
/// </summary>
public interface ITransport
{
    /// <summary>
    /// Gets the socket.
    /// Only <see cref="SocketType.Stream"/> is supported.
    /// </summary>
    Socket Socket { get; }

    /// <summary>
    /// Gets the end point.
    /// </summary>
    EndPoint EndPoint { get; }

    /// <summary>
    /// Executes before a host is started.
    /// </summary>
    void BeforeHostStart();

    /// <summary>
    /// Executes after the host binds the socket to an endpoint.
    /// The <paramref name="usedEndPoint"/> parameter contains the actually used endpoint.
    /// </summary>
    void AfterHostBind(EndPoint? usedEndPoint);

    /// <summary>
    /// Executes after a host was shut down.
    /// </summary>
    void AfterHostShutdown();
}
