// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

namespace spkl.CLI.IPC;

/// <summary>
/// Specifies where an <see cref="IListenerError"/> occurred.
/// </summary>
public enum ListenerErrorPoint
{
    /// <summary>
    /// The exception occurred when accepting a connection from the socket.
    /// This is an unrecoverable error point - the host no longer accepts connections.
    /// </summary>
    ConnectionAccept,
    /// <summary>
    /// The exception occurred when receiving information from the client.
    /// The specific client could not be serviced, but the host is still listening for new connections.
    /// </summary>
    ReceiveClientProperties,
    /// <summary>
    /// The exception occured in <see cref="IClientConnectionHandler.HandleCall(IClientConnection)"/>.
    /// This indicates a problem in the user code. The connection was closed, and the client received a <see cref="ConnectionException"/>.
    /// The host is still listening for new connections.
    /// </summary>
    ClientConnectionHandler,
}
