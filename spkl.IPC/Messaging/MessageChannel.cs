using System.Net;
using System.Net.Sockets;

namespace spkl.IPC.Messaging
{
    public class MessageChannel
    {
        public Socket Socket { get; }

        public MessageReceiver Receiver { get; }

        public MessageSender Sender { get; }

        internal MessageChannel(Socket socket)
        {
            this.Socket = socket;
            this.Receiver = new MessageReceiver(this, socket);
            this.Sender = new MessageSender(this, socket);
        }

        public void Close()
        {
            this.Socket.Close();
        }

        public static MessageChannel ConnectTo(string filePath)
        {
            Socket? socket = null;
            try
            {
                socket = GetSocket();
                socket.Connect(GetEndPoint(filePath));
            }
            catch (SocketException)
            {
                socket?.Close();
                throw;
            }

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
