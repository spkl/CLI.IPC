using spkl.IPC;
using System;
using System.Linq;
using System.Threading;

namespace Server
{
    internal class Program
    {
        private const string Path = @"C:\Users\Sebastian\Documents\Projects\StreamTest\Server\bin\Debug\net6.0\socket";

        static void Main(string[] args)
        {
            Host.Start(Path, new ClientHandler());
        }

        private class ClientHandler : IClientHandler
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
        }
    }
}