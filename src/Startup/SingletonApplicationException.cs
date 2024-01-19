using System;

namespace spkl.IPC.Startup;

public class SingletonApplicationException : Exception
{
    public SingletonApplicationException(string? message) : base(message)
    {
    }
}
