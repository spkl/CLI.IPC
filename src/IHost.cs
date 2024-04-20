// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading;

namespace spkl.CLI.IPC;

/// <summary>
/// The host (or server).
/// </summary>
public interface IHost
{
    /// <summary>
    /// Gets the number of currently connected clients.
    /// </summary>
    int ConnectedClients { get; }

    /// <summary>
    /// Gets the time the last client disconnected, expressed as UTC.
    /// </summary>
    DateTime? LastClientDisconnectTime { get; }

    /// <summary>
    /// Stops listening for incoming connections.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Returns only when no client is connected anymore.
    /// Call only after shutdown, and only once.
    /// </summary>
    /// <exception cref="InvalidOperationException">This method was called before <see cref="Shutdown"/> was called, or it was called more than once.</exception>
    void WaitUntilAllClientsDisconnected();

    /// <summary>
    /// Returns when no client is connected anymore, or the <paramref name="timeout"/> has been reached.
    /// Call only after shutdown, and only once.
    /// </summary>
    /// <exception cref="InvalidOperationException">This method was called before <see cref="Shutdown"/> was called, or it was called more than once.</exception>
    void WaitUntilAllClientsDisconnected(TimeSpan timeout);

    /// <summary>
    /// Returns when no client is connected anymore, the <paramref name="timeout"/> has been reached, or the <paramref name="cancellationToken"/> has been canceled.
    /// Call only after shutdown, and only once.
    /// </summary>
    /// <exception cref="InvalidOperationException">This method was called before <see cref="Shutdown"/> was called, or it was called more than once.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    void WaitUntilAllClientsDisconnected(TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Returns when no client was connected for a period of <paramref name="idleTime"/>.
    /// Accuracy is ~1 second.
    /// </summary>
    void WaitUntilUnusedFor(TimeSpan idleTime);

    /// <summary>
    /// Returns when no client was connected for a period of <paramref name="idleTime"/>, or the <paramref name="cancellationToken"/> has been canceled.
    /// Accuracy is ~1 second.
    /// </summary>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled.</exception>
    void WaitUntilUnusedFor(TimeSpan idleTime, CancellationToken cancellationToken);
}
