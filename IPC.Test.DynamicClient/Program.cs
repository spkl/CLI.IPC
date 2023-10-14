using System;
using System.Reflection;

namespace spkl.IPC.Test.DynamicClient;

internal class Program
{
    static void Main(string[] args)
    {
        string socketPath = args[0];
        string dllPath = args[1];
        string typeName = args[2];

        Type hostConnectionHandlerType = Assembly.LoadFrom(dllPath).GetType(typeName) ?? throw new Exception($"Could not find type {typeName} in DLL {dllPath}.");
        IHostConnectionHandler hostConnectionHandler = (IHostConnectionHandler)Activator.CreateInstance(hostConnectionHandlerType)!;

        Client.Attach(socketPath, hostConnectionHandler);
    }
}