using System.Reflection;
using System;

namespace spkl.IPC.Test.DynamicHost;

internal class Program
{
    static void Main(string[] args)
    {
        string socketPath = args[0];
        string dllPath = args[1];
        string typeName = args[2];

        Type clientConnectionHandlerType = Assembly.LoadFrom(dllPath).GetType(typeName) ?? throw new Exception($"Could not find type {typeName} in DLL {dllPath}.");
        IClientConnectionHandler clientConnectionHandler = (IClientConnectionHandler)Activator.CreateInstance(clientConnectionHandlerType)!;

        Host host = Host.Start(new UdsTransport(socketPath), clientConnectionHandler);
        Console.ReadLine();
        host.Shutdown();
    }
}