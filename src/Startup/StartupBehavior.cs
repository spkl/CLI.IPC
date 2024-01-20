// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.IPC.Startup;
public class StartupBehavior : IStartupBehavior
{
    private readonly Action startInstanceAction;

    public StartupBehavior(string negotiationFileBasePath, TimeSpan pollingPeriod, TimeSpan timeoutThreshold, Action startInstanceAction)
    {
        this.NegotiationFileBasePath = negotiationFileBasePath;
        this.PollingPeriod = pollingPeriod;
        this.TimeoutThreshold = timeoutThreshold;
        this.startInstanceAction = startInstanceAction;
    }

    public string NegotiationFileBasePath { get; }

    public TimeSpan PollingPeriod { get; }

    public TimeSpan TimeoutThreshold { get; }

    public void StartInstance()
    {
        this.startInstanceAction();
    }
}
