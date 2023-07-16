﻿using spkl.IPC.Messaging;
using System;
using System.Net.Sockets;

namespace spkl.IPC
{
    public class Client
    {
        public string FilePath { get; private set; }

        private MessageChannel Channel { get; set; }

        public IHostConnectionHandler Handler { get; private set; }

        private Client()
        {
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

            Client client = new() 
            {
                FilePath = filePath,
                Channel = channel,
                Handler = handler
            };

            try
            {
                client.SendClientProperties();
                client.RunReceiveLoop();
            }
            catch (SocketException e)
            {
                throw new Exception($"The connection was closed unexpectedly. Reason: {e.Message}"); // TODO exception type
            }
            finally
            {
                channel.Close();
            }
        }

        private void SendClientProperties()
        {
            this.Channel.Receiver.ReceiveReqArgs();
            this.Channel.Sender.SendArgs(Environment.GetCommandLineArgs());

            this.Channel.Receiver.ReceiveReqCurrentDir();
            this.Channel.Sender.SendCurrentDir(Environment.CurrentDirectory);
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
                throw new Exception("Did not receive exit message"); // TODO exception type
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
                throw new Exception($"Received unexpected message type {messageType}"); // TODO exception type
            }
        }
    }
}
