// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Public interface for the <see cref="SingletonApplication"/> class.
/// </summary>
public interface ISingletonApplication
{
    /// <summary>
    /// Ensures that a hosting application is running. If there is no running hosting application, one instance is started.
    /// </summary>
    /// <exception cref="SingletonApplicationException">There was no running hosting application within the <see cref="IStartupBehavior.TimeoutThreshold"/>.</exception>
    void RequestInstance();

    /// <summary>
    /// Reports that this application is a hosting application that is ready for incoming connections.
    /// </summary>
    /// <exception cref="SingletonApplicationException">The 'running' lock could not be obtained within <see cref="IStartupBehavior.TimeoutThreshold"/>.</exception>
    void ReportInstanceRunning();

    /// <summary>
    /// Reports that this hosting application is being shut down.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="ReportInstanceRunning"/> was not called before <see cref="ShutdownInstance"/>.
    /// It is not possible to shut down an instance that was not running.
    /// </exception>
    void ShutdownInstance();
}
