using spkl.IPC;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

            MessageChannelHost host = new MessageChannelHost(Path,
                channel =>
                    {
                        Console.WriteLine("Accepted connection");
                        for (int i = 0; i < 100; i++)
                        {
                            string str = $"abcdefghijklmnopqrstuvwxyz {i}{Environment.NewLine}";

                            SendOutString(channel.Socket, str);
                            SendOutString(channel.Socket, "");

                            Thread.Sleep(100);
                        }

                        Console.WriteLine("Closing connection");
                        channel.Socket.Close();
                    });
        }

        private static void SendOutString(Socket socket, string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            EnsureSend(socket, new byte[] { (byte)MessageType.OutString });
            EnsureSend(socket, BitConverter.GetBytes(buffer.Length));
            EnsureSend(socket, buffer);
        }

        private static void EnsureSend(Socket socket, ReadOnlySpan<byte> buffer)
        {
            int bytesSent = 0;
            while (bytesSent < buffer.Length)
            {
                bytesSent += socket.Send(buffer[bytesSent..]);
            }
        }
    }
}