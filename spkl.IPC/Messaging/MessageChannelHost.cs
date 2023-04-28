﻿using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Messaging
{
    public class MessageChannelHost
    {
        public Socket Socket { get; }

        private Action<MessageChannel> handleNewConnection;

        private Thread listenerThread;

        public MessageChannelHost(string filePath, Action<MessageChannel> handleNewConnection)
        {
            this.Socket = MessageChannel.GetSocket();
            this.Socket.Bind(MessageChannel.GetEndPoint(filePath));
            
            this.handleNewConnection = handleNewConnection;
        }

        public void AcceptConnections()
        {
            this.Socket.Listen();
            this.listenerThread = new Thread(new ThreadStart(this.Accept));
            this.listenerThread.Name = $"{nameof(MessageChannelHost)} listener for {this.Socket.LocalEndPoint}";
            this.listenerThread.Start();
        }

        public void Shutdown()
        {
            this.Socket.Close();
        }

        private void Accept()
        {
            while (true)
            {
                Socket incoming = this.Socket.Accept();
                MessageChannel messageChannel = new MessageChannel(incoming);
                Task.Factory.StartNew(() => this.handleNewConnection(messageChannel));
            }
        }
    }
}
