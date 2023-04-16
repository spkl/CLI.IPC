using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using spkl.IPC;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MessageChannel channel = MessageChannel.ConnectTo(@"C:\Users\Sebastian\Documents\Projects\StreamTest\Server\bin\Debug\net6.0\socket");

            MessageType messageType;
            while ((messageType = channel.Receiver.ReceiveMessage()) != MessageType.ConnectionClosed)
            {
                if (messageType == MessageType.OutString)
                {
                    ReadOnlySpan<byte> lengthBytes = channel.Receiver.ExpectBytes(sizeof(int));
                    int length = BitConverter.ToInt32(lengthBytes);

                    ReadOnlySpan<byte> messageBytes = channel.Receiver.ExpectBytes(length);
                    string message = Encoding.UTF8.GetString(messageBytes);

                    Console.Write(message);
                }
            }

            Thread.Sleep(3000);
        }
    }
}