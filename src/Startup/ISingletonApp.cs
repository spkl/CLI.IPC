// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Public interface for the <see cref="SingletonApp"/> class.
/// </summary>
public interface ISingletonApp
{
    /// <summary>
    /// Ensures that a hosting application is running. If there is no running hosting application, one instance is started.
    /// </summary>
    /// <exception cref="SingletonAppException">There was no running hosting application within the <see cref="IStartupBehavior.TimeoutThreshold"/>.</exception>
    void RequestInstance();

    /// <summary>
    /// Reports that this application is a hosting application that is ready for incoming connections.
    /// </summary>
    /// <exception cref="SingletonAppException">The 'running' lock could not be obtained within <see cref="IStartupBehavior.TimeoutThreshold"/>.</exception>
    void ReportInstanceRunning();

    /// <summary>
    /// Reports that this hosting application is being shut down.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="ReportInstanceRunning"/> was not called before <see cref="ShutdownInstance"/>.
    /// It is not possible to shut down an instance that was not running.
    /// </exception>
    void ShutdownInstance();

    /// <summary>
    /// Prevents the start of a new hosting application instance until <see cref="IDisposable.Dispose"/> is called on the returned object.
    /// Calls to <see cref="RequestInstance"/> during this period will time out, if there is no instance running.
    /// </summary>
    /// <exception cref="SingletonAppException">
    /// The startup lock could not be obtained within <see cref="IStartupBehavior.TimeoutThreshold"/>.
    /// </exception>
    IDisposable SuspendStartup();

    /// <summary>
    /// Returns whether a hosting application is currently running.
    /// </summary>
    bool IsInstanceRunning();

    /// <summary>
    /// Returns whether a hosting application is currently starting.
    /// </summary>
    bool IsInstanceStarting();
}
