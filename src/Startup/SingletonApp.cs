// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Internal;
using System;
using System.Threading;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// Ensures that an application is only started once.
/// Usage as a client application: Call <see cref="RequestInstance"/> to ensure that an application instance is running before connecting to it.
/// Usage as the hosting application: Call <see cref="ReportInstanceRunning"/> when ready for incoming connections. Call <see cref="ReportInstanceShuttingDown"/> before exit.
/// </summary>
public sealed class SingletonApp : IDisposable, ISingletonApp
{
    /// <summary>
    /// Occurs before polling for a running or starting instance.
    /// </summary>
    public EventHandler? BeforeRequestingInstance;

    private readonly IStartupBehavior behavior;

    private readonly FileLock startupLock;

    private readonly FileLock runningLock;

    private bool isThisInstanceStarting;

    private readonly Random random;

    private readonly int pollingPeriodMin, pollingPeriodMax;

    /// <summary>
    /// </summary>
    public SingletonApp(IStartupBehavior behavior)
    {
        this.behavior = behavior;

        this.startupLock = new FileLock(this.behavior.NegotiationFileBasePath + ".start_lock");
        this.runningLock = new FileLock(this.behavior.NegotiationFileBasePath + ".run_lock");

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

            if (this.IsInstanceRunning())
            {
                return true;
            }

            if (!this.IsInstanceStarting())
            {
                this.TryToStart();
            }

            int millisecondsTimeout = this.random.Next(this.pollingPeriodMin, this.pollingPeriodMax);
            Thread.Sleep(millisecondsTimeout);
            return false;
        });

        if (this.isThisInstanceStarting)
        {
            this.startupLock.Unlock();
            this.isThisInstanceStarting = false;
        }

        if (!this.IsInstanceRunning())
        {
            throw new SingletonAppException($"Timed out: Application did not become available within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <inheritdoc/>
    public void ReportInstanceRunning()
    {
        if (!this.runningLock.TryLock(this.behavior.TimeoutThreshold))
        {
            throw new SingletonAppException($"Timed out: Could not get lock on {this.runningLock.Path} within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <inheritdoc/>
    [Obsolete("Use ReportInstanceShuttingDown() instead, as its method name has clearer wording.")]
    public void ShutdownInstance()
    {
        this.ReportInstanceShuttingDown();
    }

    /// <inheritdoc/>
    public void ReportInstanceShuttingDown()
    {
        if (!this.runningLock.IsHoldingLock())
        {
            throw new InvalidOperationException($"{nameof(ReportInstanceRunning)} must be called before {nameof(ReportInstanceShuttingDown)}.");
        }

        this.runningLock.Unlock();
    }

    /// <inheritdoc/>
    public IDisposable SuspendStartup()
    {
        if (!this.startupLock.TryLock(this.behavior.TimeoutThreshold))
        {
            throw new SingletonAppException($"Timed out: Could not get lock on {this.startupLock.Path} within {this.behavior.TimeoutThreshold}.");
        }

        return new Disposable(() => this.startupLock.Unlock());
    }

    /// <inheritdoc/>
    public bool IsInstanceRunning()
    {
        return this.runningLock.IsLocked();
    }

    /// <inheritdoc/>
    public bool IsInstanceStarting()
    {
        if (this.isThisInstanceStarting)
        {
            return true;
        }

        return this.startupLock.IsLocked();
    }

    private void TryToStart()
    {
        if (!this.startupLock.TryLock())
        {
            return;
        }

        this.isThisInstanceStarting = true;
        this.behavior.StartInstance();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.startupLock.Unlock();
        this.runningLock.Unlock();
    }
}
