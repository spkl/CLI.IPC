using System;
using System.Net.Sockets;
using System.Text;

namespace spkl.IPC
{
    public class MessageSender
    {
        public MessageChannel Channel { get; }

        public Socket Socket { get; }

        internal MessageSender(MessageChannel channel, Socket socket)
        {
            this.Channel = channel;
            this.Socket = socket;
        }

        public void SendReqArgs()
        {
            this.SendMessageType(MessageType.ReqArgs);
        }

        public void SendArgs(string[] args)
        {
            this.SendMessageType(MessageType.Args);
            this.SendInt(args.Length);
            foreach (string arg in args) 
            {
                this.SendString(arg);
            }
        }

        public void SendOutStr(string str)
        {
            this.SendMessageType(MessageType.OutStr);
            this.SendString(str);
        }

        public void SendErrStr(string str)
        {
            this.SendMessageType(MessageType.ErrStr);
            this.SendString(str);
        }

        public void SendExit(int exitCode)
        {
            this.SendMessageType(MessageType.Exit);
            this.SendInt(exitCode);
        }

        private void SendMessageType(MessageType type)
        {
            this.SendBytes(new byte[] { (byte)type });
        }

        private void SendString(string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            this.SendInt(buffer.Length);
            this.SendBytes(buffer);
        }

        private void SendInt(int n)
        {
            this.SendBytes(BitConverter.GetBytes(n));
        }

        private void SendBytes(ReadOnlySpan<byte> buffer)
        {
            int bytesSent = 0;
            while (bytesSent < buffer.Length)
            {
                bytesSent += this.Socket.Send(buffer[bytesSent..]);
            }
        }
    }
}
