using System.Reflection;
using System;

namespace spkl.IPC.Test.DynamicHost;

internal class Program
{
    static void Main(string[] args)
    {
        string transportArgument = args[0];
        string dllPath = args[1];
        string typeName = args[2];

        Type clientConnectionHandlerType = Type.GetType(typeName + ", spkl.IPC, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e5ae298f5c89d3e9")
                                           ?? Assembly.LoadFrom(dllPath).GetType(typeName)
                                           ?? throw new Exception($"Could not find type {typeName} in DLL {dllPath}.");
        IClientConnectionHandler clientConnectionHandler = (IClientConnectionHandler)Activator.CreateInstance(clientConnectionHandlerType)!;

        ITransport transport;
#if NET6_0_OR_GREATER
        transport = new UdsTransport(transportArgument);
#else
        transport = new TcpLoopbackTransport(int.Parse(transportArgument));
#endif

        Host host = Host.Start(transport, clientConnectionHandler);
        Console.ReadLine();
        host.Shutdown();
    }
}