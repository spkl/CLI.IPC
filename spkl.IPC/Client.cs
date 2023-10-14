﻿using spkl.IPC.Messaging;
using System;
using System.Net.Sockets;

namespace spkl.IPC
{
    public class Client
    {
        public string FilePath { get; }

        private MessageChannel Channel { get; }

        public IHostConnectionHandler Handler { get; }

        private Client(string filePath, MessageChannel channel, IHostConnectionHandler handler)
        {
            this.FilePath = filePath;
            this.Channel = channel;
            this.Handler = handler;
        }

        public static void Attach(string filePath, IHostConnectionHandler handler)
        {
            MessageChannel channel;
            try
            {
                channel = MessageChannel.ConnectTo(filePath);
            }
            catch (SocketException e)
            {
                throw new Exception($"Could not connect. Reason: {e.Message}");
            }

            Client client = new(filePath, channel, handler);

            try
            {
                client.SendClientProperties();
                client.RunReceiveLoop();
            }
            catch (SocketException e)
            {
                throw new ConnectionException($"There was an unexpected connection error. Reason: {e.Message}", e);
            }
            finally
            {
                channel.Close();
            }
        }

        private void SendClientProperties()
        {
            this.Channel.Receiver.ReceiveReqArgs();
            this.Channel.Sender.SendArgs(this.Handler.Arguments);

            this.Channel.Receiver.ReceiveReqCurrentDir();
            this.Channel.Sender.SendCurrentDir(this.Handler.CurrentDirectory);
        }

        private void RunReceiveLoop()
        {
            bool receivedExit = false;
            MessageType messageType;
            while ((messageType = this.Channel.Receiver.ReceiveMessage()) != MessageType.ConnClosed)
            {
                this.Receive(ref messageType, ref receivedExit);
            }           

            if (!receivedExit)
            {
                throw new ConnectionException("The connection was closed without receiving the exit code.");
            }
        }

        private void Receive(ref MessageType messageType, ref bool receivedExit)
        {
            if (messageType == MessageType.OutStr)
            {
                string str = this.Channel.Receiver.ExpectString();
                this.Handler.HandleOutString(str);
            }
            else if (messageType == MessageType.ErrStr)
            {
                string str = this.Channel.Receiver.ExpectString();
                this.Handler.HandleErrorString(str);
            }
            else if (messageType == MessageType.Exit)
            {
                int exitCode = this.Channel.Receiver.ExpectInt();
                this.Handler.HandleExit(exitCode);
                receivedExit = true;
            }
            else
            {
                throw new ConnectionException($"Received unexpected message type '{messageType}' after the connection was established.");
            }
        }
    }
}
