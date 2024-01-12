using spkl.IPC;
using System;
using System.Linq;
using System.Threading;

namespace Server;

internal class Program
{
    private const string Path = @"C:\Users\Sebastian\Documents\Projects\StreamTest\Server\bin\Debug\net6.0\socket";

    static void Main(string[] args)
    {
        var host = Host.Start(new UdsTransport(Path), new ClientConnectionHandler());
        Console.WriteLine("Press Enter to shutdown...");
        Console.ReadLine();
        host.Shutdown();
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
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