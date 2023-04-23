using System;
using System.Threading;
using spkl.IPC;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MessageChannel channel = MessageChannel.ConnectTo(@"C:\Users\Sebastian\Documents\Projects\StreamTest\Server\bin\Debug\net6.0\socket");
            channel.Receiver.ReceiveReqArgs();
            channel.Sender.SendArgs(args);

            bool receivedExit = false;
            MessageType messageType;
            while ((messageType = channel.Receiver.ReceiveMessage()) != MessageType.ConnClosed)
            {
                if (messageType == MessageType.OutStr)
                {
                    string str = channel.Receiver.ExpectString();
                    Console.Write(str);
                }
                else if (messageType == MessageType.ErrStr)
                {
                    string str = channel.Receiver.ExpectString();
                    Console.Error.Write(str);
                }
                else if (messageType == MessageType.Exit)
                {
                    Environment.ExitCode = channel.Receiver.ExpectInt();
                    receivedExit = true;
                }
                else
                {
                    throw new Exception($"Received unexpected message type {messageType}");
                }
            }

            if (!receivedExit)
            {
                throw new Exception("Did not receive exit message");
            }

            Thread.Sleep(3000);
        }
    }
}