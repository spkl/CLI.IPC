// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace spkl.CLI.IPC;

/// <summary>
/// Specifies how a host handles client connections and errors.
/// </summary>
public interface IClientConnectionHandler
{
    /// <summary>
    /// Gets the TaskFactory responsible for creating a task for a new incoming connection.
    /// Default: <see cref="Task.Factory"/>.
    /// </summary>
#if NET6_0_OR_GREATER
    TaskFactory TaskFactory => Task.Factory;
#else
    TaskFactory TaskFactory { get; }
#endif

    /// <summary>
    /// Handles an incoming client connection.
    /// This method must implement its own exception handling, otherwise exceptions might be left unhandled.
    /// </summary>
    /// <remarks>
    /// If the connection is not closed when this method returns (<see cref="ClientConnection.Exit(int)"/>), it is automatically closed with exit code 0.
    /// If this method throws an exception, the connection is automatically closed without sending an exit code, causing the client to report an error, too.
    /// </remarks>
    /// <param name="connection">Object to interact with the connected client.</param>
    void HandleCall(ClientConnection connection);

    /// <summary>
    /// Handles an error that occurs while listening for, accepting, or processing client connections.
    /// </summary>
    void HandleListenerError(ListenerError error);
}
