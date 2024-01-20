// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.IPC;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server;

internal class Program
{
    static void Main(string[] args)
    {
        ITransport transport;
#if NET6_0_OR_GREATER
        transport = new UdsTransport(@"C:\Users\Sebastian\Documents\Projects\StreamTest\playground\Server\bin\Debug\net6.0\socket");
#else
        transport = new TcpLoopbackTransport(65058);
#endif

        var host = Host.Start(transport, new ClientConnectionHandler());
        Console.WriteLine("Press Enter to shutdown...");
        Console.ReadLine();
        host.Shutdown();
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(ClientConnection connection)
        {
            Console.WriteLine("Accepted connection");

            string[] clientArgs = connection.Properties.Arguments;
            Console.WriteLine($"Arguments: {string.Join(" ", clientArgs.Select(arg => $@"""{arg}"""))}");

            string currentDir = connection.Properties.CurrentDirectory;
            Console.WriteLine($"CurrentDirectory: {currentDir}");


            for (int i = 0; i < 100; i++)
            {
                connection.Out.WriteLine($"abcdefghijklmnopqrstuvwxyz {i}");
                Thread.Sleep(100);
            }

            connection.Error.Write("this is an error string");
            connection.Exit(1);

            connection.Out.Write(true);
        }

        public void HandleListenerError(ListenerError error)
        {
            Console.WriteLine(error.Exception);
        }
    }
}