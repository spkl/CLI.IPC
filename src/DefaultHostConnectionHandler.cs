// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace spkl.CLI.IPC;

/// <summary>
/// The default implementation for <see cref="IHostConnectionHandler2"/>:
/// Sends <see cref="Environment.GetCommandLineArgs"/>, <see cref="Environment.CurrentDirectory"/> and the current process ID,
/// writes any received output to the <see cref="Console"/> and calls <see cref="Environment.Exit(int)"/> when the connection is closed.
/// </summary>
public class DefaultHostConnectionHandler : IHostConnectionHandler2
{
    /// <inheritdoc/>
    public virtual string[] Arguments => Environment.GetCommandLineArgs();

    /// <inheritdoc/>
    public virtual string CurrentDirectory => Environment.CurrentDirectory;

    /// <inheritdoc/>
    public virtual int ProcessID
    {
        get
        {
#if NET6_0_OR_GREATER
            return Environment.ProcessId;
#else
            using Process currentProcess = Process.GetCurrentProcess();
            return currentProcess.Id;
#endif
        }
    }

    /// <inheritdoc/>
    public virtual void HandleOutString(string text)
    {
        Console.Out.Write(text);
    }

    /// <inheritdoc/>
    public virtual void HandleErrorString(string text)
    {
        Console.Error.Write(text);
    }

    /// <inheritdoc/>
    public virtual void HandleExit(int exitCode)
    {
        Console.Out.Flush();
        Console.Error.Flush();
        Environment.Exit(exitCode);
    }
}
