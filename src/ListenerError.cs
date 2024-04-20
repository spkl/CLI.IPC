// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

/// <inheritdoc cref="IListenerError"/>
public class ListenerError : IListenerError
{
    /// <inheritdoc/>
    public Exception Exception { get; }

    /// <inheritdoc/>
    public ListenerErrorPoint ErrorPoint { get; }

    /// <inheritdoc/>
    public bool IsHostInterrupted => this.ErrorPoint == ListenerErrorPoint.ConnectionAccept;

    internal ListenerError(Exception exception, ListenerErrorPoint errorPoint)
    {
        this.Exception = exception;
        this.ErrorPoint = errorPoint;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Exception.ToString();
    }
}
