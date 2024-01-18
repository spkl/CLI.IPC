using System;
using System.IO;
using System.Threading;

namespace spkl.IPC.Startup;

/// <summary>
/// Ensures that an application is only started once.
/// Usage as a client application: Call <see cref="RequestInstance"/> to ensure that an application instance is running before connecting to it.
/// Usage as the hosting application: Call <see cref="ReportInstanceRunning"/> when ready for incoming connections. Call <see cref="ShutdownInstance"/> before exit.
/// </summary>
public class SingletonApplication
{
    private readonly IStartupBehavior behavior;

    private string StartupLockPath => this.behavior.NegotiationFileBasePath + ".lock0";

    private string RunningLockPath => this.behavior.NegotiationFileBasePath + ".lock1";

    private FileStream? startupLockStream;

    private FileStream? runningLockStream;

    public SingletonApplication(IStartupBehavior behavior)
    {
        this.behavior = behavior;
    }

    /// <summary>
    /// Ensures that a hosting application is running. If there is no running hosting application, one instance is started.
    /// </summary>
    /// <exception cref="SingletonApplicationException">There was no running hosting application within the <see cref="IStartupBehavior.TimeoutThreshold"/>.</exception>
    public void RequestInstance()
    {
        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime) < this.behavior.TimeoutThreshold)
        {
            if (this.IsRunning())
            {
                return;
            }

            if (!this.IsStarting())
            {
                this.behavior.StartInstance();
            }

            Thread.Sleep(this.behavior.PollingPeriod);
        }

        if (!this.IsRunning())
        {
            throw new SingletonApplicationException($"Timed out: Application did not become available within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <summary>
    /// Reports that this application is a hosting application that is ready for incoming connections.
    /// </summary>
    /// <exception cref="SingletonApplicationException">The 'running' lock could not be obtained within <see cref="IStartupBehavior.TimeoutThreshold"/>.</exception>
    public void ReportInstanceRunning()
    {
        DateTime startTime = DateTime.Now;
        while (this.runningLockStream == null && (DateTime.Now - startTime) < this.behavior.TimeoutThreshold)
        {
            try
            {
                this.runningLockStream = this.OpenStream(this.RunningLockPath);
            }
            catch (IOException)
            {
            }
        }

        if (this.runningLockStream == null)
        {
            throw new SingletonApplicationException($"Timed out: Could not get lock on {this.RunningLockPath} within {this.behavior.TimeoutThreshold}.");
        }
    }

    /// <summary>
    /// Reports that this hosting application is being shut down.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="ReportInstanceRunning"/> was not called before <see cref="ShutdownInstance"/>.
    /// It is not possible to shut down an instance that was not running.
    /// </exception>
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
            using FileStream _ = this.OpenStream(this.RunningLockPath);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private bool IsStarting()
    {
        if (this.startupLockStream != null)
        {
            return true;
        }

        try
        {
            this.startupLockStream = this.OpenStream(this.StartupLockPath);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private FileStream OpenStream(string path)
    {
        return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
    }
}
