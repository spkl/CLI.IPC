// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

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

    private string StartupLockPath => this.behavior.NegotiationFileBasePath + ".lock0";

    private string RunningLockPath => this.behavior.NegotiationFileBasePath + ".lock1";

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
        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime) < this.behavior.TimeoutThreshold)
        {
            this.BeforeRequestingInstance?.Invoke(this, EventArgs.Empty);

            if (this.IsRunning())
            {
                return;
            }

            if (!this.IsStarting())
            {
                this.TryToStart();
            }

            int millisecondsTimeout = this.random.Next(this.pollingPeriodMin, this.pollingPeriodMax);
            Thread.Sleep(millisecondsTimeout);
        }

        if (!this.IsRunning())
        {
            throw new SingletonAppException($"Timed out: Application did not become available within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <inheritdoc/>
    public void ReportInstanceRunning()
    {
        DateTime startTime = DateTime.Now;
        while (this.runningLockStream == null && (DateTime.Now - startTime) < this.behavior.TimeoutThreshold)
        {
            try
            {
                this.runningLockStream = this.OpenStreamForLocking(this.RunningLockPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

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

        this.runningLockStream?.Dispose();
        this.runningLockStream = null;
    }

    private bool IsRunning()
    {
        try
        {
            this.OpenStreamForChecking(this.RunningLockPath);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }

    private bool IsStarting()
    {
        if (this.isThisInstanceStarting)
        {
            return true;
        }

        try
        {
            this.OpenStreamForChecking(this.StartupLockPath);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }

    private void TryToStart()
    {
        try
        {
            this.startupLockStream = this.OpenStreamForLocking(this.StartupLockPath);
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        this.isThisInstanceStarting = true;
        this.behavior.StartInstance();
    }

    private void OpenStreamForChecking(string path)
    {
        new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.DeleteOnClose).Dispose();
    }

    private FileStream OpenStreamForLocking(string path)
    {
        return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.startupLockStream?.Dispose();
        this.runningLockStream?.Dispose();
    }
}
