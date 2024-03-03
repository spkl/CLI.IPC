// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC.Startup;

/// <summary>
/// The default implementation for <see cref="IStartupBehavior"/>.
/// All aspects are defined by the constructor parameters.
/// </summary>
public class StartupBehavior : IStartupBehavior
{
    private readonly Action startInstanceAction;

    /// <summary>
    /// </summary>
    public StartupBehavior(string negotiationFileBasePath, TimeSpan pollingPeriod, TimeSpan timeoutThreshold, Action startInstanceAction)
    {
        this.NegotiationFileBasePath = negotiationFileBasePath;
        this.PollingPeriod = pollingPeriod;
        this.TimeoutThreshold = timeoutThreshold;
        this.startInstanceAction = startInstanceAction;
    }

    /// <inheritdoc/>
    public string NegotiationFileBasePath { get; }

    /// <inheritdoc/>
    public TimeSpan PollingPeriod { get; }

    /// <inheritdoc/>
    public TimeSpan TimeoutThreshold { get; }

    /// <inheritdoc/>
    public void StartInstance()
    {
        this.startInstanceAction();
    }
}
