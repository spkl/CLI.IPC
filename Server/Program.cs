using spkl.IPC;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Server
{
    internal class Program
    {
        private const string Path = @"C:\Users\Sebastian\Documents\Projects\StreamTest\Server\bin\Debug\net6.0\socket";

        static void Main(string[] args)
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }

            MessageChannelHost host = new MessageChannelHost(Path, HandleNewConnection);
            host.AcceptConnections();
        }

        private static void HandleNewConnection(MessageChannel channel)
        {
            Console.WriteLine("Accepted connection");

            channel.Sender.SendReqArgs();
            string[] clientArgs = channel.Receiver.ReceiveArgs();
            Console.WriteLine($"Received args: {string.Join(" ", clientArgs.Select(arg => $@"""{arg}"""))}");

            for (int i = 0; i < 100; i++)
            {
                string str = $"abcdefghijklmnopqrstuvwxyz {i}{Environment.NewLine}";

                channel.Sender.SendOutStr(str);

                Thread.Sleep(100);
            }

            channel.Sender.SendErrStr("this is an error string");
            channel.Sender.SendExit(1);

            Console.WriteLine("Closing connection");
            channel.Close();
        }
    }
}