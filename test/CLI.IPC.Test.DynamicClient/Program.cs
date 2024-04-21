using System;
using System.Reflection;

namespace spkl.CLI.IPC.Test.DynamicClient;

internal class Program
{
    static void Main(string[] args)
    {
        string transportArgument = args[0];
        string dllPath = args[1];
        string typeName = args[2];

        Type hostConnectionHandlerType = Type.GetType(typeName + ", spkl.CLI.IPC, Version=2.0.0.0, Culture=neutral, PublicKeyToken=e5ae298f5c89d3e9")
                                         ?? Assembly.LoadFrom(dllPath).GetType(typeName)
                                         ?? throw new Exception($"Could not find type {typeName} in DLL {dllPath}.");
        IHostConnectionHandler hostConnectionHandler = (IHostConnectionHandler)Activator.CreateInstance(hostConnectionHandlerType)!;

        ITransport transport;
#if NET6_0_OR_GREATER
        transport = new UdsTransport(transportArgument);
#else
        transport = new TcpLoopbackTransport(int.Parse(transportArgument));
#endif

        Client.Attach(transport, hostConnectionHandler);
    }
}
