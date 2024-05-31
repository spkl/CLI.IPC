// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC;
using spkl.CLI.IPC.Startup;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server;

internal class Program
{
    static void Main(string[] args)
    {
        AutoTransportSingletonApp app = new AutoTransportSingletonApp(
            new StartupBehavior(
                @"C:\Users\Sebastian\Documents\Projects\spkl.CLI.IPC\playground\singleton",
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromSeconds(5),
                () => { }));

        Host host = Host.Start(app.Transport, new ClientConnectionHandler());
        app.ReportInstanceRunning();
        Console.WriteLine($"Listening on {app.Transport.EndPoint}");

        //Console.WriteLine("Press Enter to shutdown...");
        //Console.ReadLine();
        Console.WriteLine("Waiting until unused for 10 seconds...");
        host.WaitUntilUnusedFor(TimeSpan.FromSeconds(10));

        app.ReportInstanceShuttingDown();
        host.Shutdown();
        host.WaitUntilAllClientsDisconnected();
    }

    private class ClientConnectionHandler : IClientConnectionHandler
    {
        public TaskFactory TaskFactory => Task.Factory;

        public void HandleCall(IClientConnection connection)
        {
            Console.WriteLine("Accepted connection");

            string[] clientArgs = connection.Properties.Arguments;
            Console.WriteLine($"Arguments: {string.Join(" ", clientArgs.Select(arg => $@"""{arg}"""))}");

            string currentDir = connection.Properties.CurrentDirectory;
            Console.WriteLine($"CurrentDirectory: {currentDir}");

            int processId = connection.Properties.ProcessID;
            Console.WriteLine($"ProcessID: {processId}");


            for (int i = 0; i < 100; i++)
            {
                connection.Out.WriteLine($"abcdefghijklmnopqrstuvwxyz {i}");
                Thread.Sleep(100);
            }

            connection.Error.Write("this is an error string");
            connection.Exit(1);

            Console.WriteLine("Closed connection");
            //connection.Out.Write(true);
        }

        public void HandleListenerError(IListenerError error)
        {
            Console.WriteLine(error.Exception);
        }
    }
}
