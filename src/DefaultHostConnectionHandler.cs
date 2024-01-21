// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

/// <summary>
/// The default implementation for <see cref="IHostConnectionHandler"/>:
/// Sends <see cref="Environment.GetCommandLineArgs"/> and <see cref="Environment.CurrentDirectory"/>,
/// writes any received output to the <see cref="Console"/> and calls <see cref="Environment.Exit(int)"/> when the connection is closed.
/// </summary>
public class DefaultHostConnectionHandler : IHostConnectionHandler
{
    /// <inheritdoc/>
    public virtual string[] Arguments => Environment.GetCommandLineArgs();

    /// <inheritdoc/>
    public virtual string CurrentDirectory => Environment.CurrentDirectory;

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
