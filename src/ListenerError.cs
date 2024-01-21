// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

public class ListenerError
{
    /// <summary>
    /// The exception that occurred.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Indicates at which point in the connection process the error occured.
    /// </summary>
    public ListenerErrorPoint ErrorPoint { get; }

    /// <summary>
    /// If this is true, the <see cref="Host"/> is no longer accepts connections and should be shut down.
    /// </summary>
    public bool IsHostInterrupted => this.ErrorPoint == ListenerErrorPoint.ConnectionAccept;

    public ListenerError(Exception exception, ListenerErrorPoint errorPoint)
    {
        this.Exception = exception;
        this.ErrorPoint = errorPoint;
    }

    public override string ToString()
    {
        return this.Exception.ToString();
    }
}
