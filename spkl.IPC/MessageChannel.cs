using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace spkl.IPC
{
    public class MessageChannel
    {
        public Socket Socket { get; }

        public MessageReceiver Receiver { get; }

        public MessageSender Sender { get; }

        internal MessageChannel(Socket socket)
        {
            this.Socket = socket;
            this.Receiver = new MessageReceiver(socket);
            this.Sender = new MessageSender(socket);
        }

        public void Close()
        {
            this.Socket.Close();
        }

        public static MessageChannel ConnectTo(string filePath)
        {
            Socket socket = GetSocket();
            socket.Connect(GetEndPoint(filePath));

            return new MessageChannel(socket);
        }

        internal static Socket GetSocket()
        {
            return new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        }

        internal static EndPoint GetEndPoint(string filePath)
        {
            return new UnixDomainSocketEndPoint(filePath);
        }
    }
}
