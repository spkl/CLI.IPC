using System;

namespace spkl.IPC.Startup;

public interface IStartupBehavior
{
    string NegotiationFileBasePath { get; }

    void StartInstance();

    TimeSpan PollingPeriod { get; }

    TimeSpan TimeoutThreshold { get; }
}
