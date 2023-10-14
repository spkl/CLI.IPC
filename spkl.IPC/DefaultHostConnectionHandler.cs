﻿using System;

namespace spkl.IPC
{
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
            Environment.Exit(exitCode);
        }
    }
}
