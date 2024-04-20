// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Internal;
using System;
using System.IO;
using System.Threading;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Ensures that an application is only started once.
/// Usage as a client application: Call <see cref="RequestInstance"/> to ensure that an application instance is running before connecting to it.
/// Usage as the hosting application: Call <see cref="ReportInstanceRunning"/> when ready for incoming connections. Call <see cref="ShutdownInstance"/> before exit.
/// </summary>
public sealed class SingletonApp : IDisposable, ISingletonApp
{
    /// <summary>
    /// Occurs before polling for a running or starting instance.
    /// </summary>
    public EventHandler? BeforeRequestingInstance;

    private readonly IStartupBehavior behavior;

    private string StartupLockPath => this.behavior.NegotiationFileBasePath + ".start_lock";

    private string RunningLockPath => this.behavior.NegotiationFileBasePath + ".run_lock";

    private FileStream? startupLockStream;

    private FileStream? runningLockStream;

    private bool isThisInstanceStarting;

    private readonly Random random;

    private readonly int pollingPeriodMin, pollingPeriodMax;

    /// <summary>
    /// </summary>
    public SingletonApp(IStartupBehavior behavior)
    {
        this.behavior = behavior;

        this.random = new Random();

        int pollingPeriod = (int)this.behavior.PollingPeriod.TotalMilliseconds;
        const double pollingPeriodVariability = 0.25;
        this.pollingPeriodMin = (int)(pollingPeriod * (1.0 - pollingPeriodVariability));
        this.pollingPeriodMax = (int)(pollingPeriod * (1.0 + pollingPeriodVariability));
    }

    /// <inheritdoc/>
    public void RequestInstance()
    {
        Try.UntilTimedOut(this.behavior.TimeoutThreshold, () =>
        {
            this.BeforeRequestingInstance?.Invoke(this, EventArgs.Empty);

            if (this.IsRunning())
            {
                return true;
            }

            if (!this.IsStarting())
            {
                this.TryToStart();
            }

            int millisecondsTimeout = this.random.Next(this.pollingPeriodMin, this.pollingPeriodMax);
            Thread.Sleep(millisecondsTimeout);
            return false;
        });

        if (this.isThisInstanceStarting)
        {
            Try.Dispose(ref this.startupLockStream);
            this.isThisInstanceStarting = false;
        }

        if (!this.IsRunning())
        {
            throw new SingletonAppException($"Timed out: Application did not become available within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <inheritdoc/>
    public void ReportInstanceRunning()
    {
        this.runningLockStream = FileStreams.TryLock(this.RunningLockPath, this.behavior.TimeoutThreshold);

        if (this.runningLockStream == null)
        {
            throw new SingletonAppException($"Timed out: Could not get lock on {this.RunningLockPath} within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <inheritdoc/>
    public void ShutdownInstance()
    {
        if (this.runningLockStream == null)
        {
            throw new InvalidOperationException($"{nameof(ReportInstanceRunning)} must be called before {nameof(ShutdownInstance)}.");
        }

        Try.Dispose(ref this.runningLockStream);
    }

    private bool IsRunning()
    {
        return FileStreams.IsLocked(this.RunningLockPath);
    }

    private bool IsStarting()
    {
        if (this.isThisInstanceStarting)
        {
            return true;
        }

        return FileStreams.IsLocked(this.StartupLockPath);
    }

    private void TryToStart()
    {
        this.startupLockStream = FileStreams.TryLock(this.StartupLockPath);
        if (this.startupLockStream == null)
        {
            return;
        }

        this.isThisInstanceStarting = true;
        this.behavior.StartInstance();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Try.Dispose(ref this.startupLockStream);
        Try.Dispose(ref this.runningLockStream);
    }
}
