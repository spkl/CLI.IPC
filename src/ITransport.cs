// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;

namespace spkl.IPC;

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
    /// Executes after a host was shut down.
    /// </summary>
    void AfterHostShutdown();
}
