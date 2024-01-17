using System;
using System.IO;
using System.Threading;

namespace spkl.IPC.Startup;
public class Singleton
{
    private readonly IStartupBehavior behavior;

    private string StartupLockPath => this.behavior.NegotiationFileBasePath + ".lock0";

    private string RunningLockPath => this.behavior.NegotiationFileBasePath + ".lock1";

    private FileStream? startupLockStream;

    private FileStream? runningLockStream;

    public Singleton(IStartupBehavior behavior)
    {
        this.behavior = behavior;
    }

    public void RequestInstance()
    {
        DateTime startTime = DateTime.Now;
        try
        {
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
        }
        finally
        {
            this.startupLockStream?.Dispose();
            this.startupLockStream = null;
        }
        
        if (!this.IsRunning())
        {
            throw new Exception("Timed out");
        }
    }

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
            throw new Exception("Timed out");
        }
    }

    public void ShutdownInstance()
    {
        if (this.runningLockStream == null)
        {
            throw new InvalidOperationException();
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
