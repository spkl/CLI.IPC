// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Controls behavior and parameters for <see cref="SingletonApp"/>s.
/// </summary>
public interface IStartupBehavior
{
    /// <summary>
    /// Gets the base portion of the file names used for negotiating singleton startup/running.
    /// E. g., if C:\foo\bar is returned, the files C:\foo\bar.lock0 and C:\foo\bar.lock1 will be used.
    /// Write-access must be available in the directory.
    /// </summary>
    string NegotiationFileBasePath { get; }

    /// <summary>
    /// Starts a new instance of a hosting application.
    /// Typically, this starts a new process with an argument that indicates that the instance should behome a host.
    /// </summary>
    void StartInstance();

    /// <summary>
    /// Gets the period that is used for polling whether a hosting application is running (<see cref="SingletonApp.RequestInstance"/>).
    /// The actually used period is randomized within +/- 25% from this value.
    /// </summary>
    TimeSpan PollingPeriod { get; }

    /// <summary>
    /// Gets the period that is used until a time out occurs during <see cref="SingletonApp.RequestInstance"/> and <see cref="SingletonApp.ReportInstanceRunning"/>.
    /// <b>Attention:</b> This implies that the hosting application is expected to start and become ready for incoming connections within this time period.
    /// </summary>
    TimeSpan TimeoutThreshold { get; }
}
