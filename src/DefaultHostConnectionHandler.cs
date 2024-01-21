// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace spkl.CLI.IPC;

public class DefaultHostConnectionHandler : IHostConnectionHandler
{
    public virtual string[] Arguments => Environment.GetCommandLineArgs();

    public virtual string CurrentDirectory => Environment.CurrentDirectory;

    public virtual void HandleOutString(string text)
    {
        Console.Out.Write(text);
    }

    public virtual void HandleErrorString(string text)
    {
        Console.Error.Write(text);
    }

    public virtual void HandleExit(int exitCode)
    {
        Console.Out.Flush();
        Console.Error.Flush();
        Environment.Exit(exitCode);
    }
}
