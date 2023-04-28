using System;
using System.Net.Sockets;
using System.Text;

namespace spkl.IPC.Messaging
{
    public class MessageSender
    {
        public MessageChannel Channel { get; }

        public Socket Socket { get; }

        private byte[] buffer;

        internal MessageSender(MessageChannel channel, Socket socket)
        {
            this.Channel = channel;
            this.Socket = socket;
            this.buffer = new byte[4];
        }

        private void EnsureBufferSize(int requiredSize)
        {
            if (this.buffer.Length < requiredSize)
            {
                this.buffer = new byte[requiredSize];
            }
        }

        public void SendReqArgs()
        {
            this.SendMessageType(MessageType.ReqArgs);
        }

        public void SendReqCurrentDir()
        {
            this.SendMessageType(MessageType.ReqCurrentDir);
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

        public void SendCurrentDir(string currentDir)
        {
            this.SendMessageType(MessageType.CurrentDir);
            this.SendString(currentDir);
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
            Span<byte> messageTypeBuffer = this.buffer.AsSpan(0, 1);
            messageTypeBuffer[0] = (byte)type;
            this.SendBytes(messageTypeBuffer);
        }

        private void SendString(string str)
        {
            int byteCount = Encoding.UTF8.GetByteCount(str);
            this.SendInt(byteCount);

            this.EnsureBufferSize(byteCount);
            Encoding.UTF8.GetBytes(str, 0, str.Length, this.buffer, 0);
            this.SendBytes(this.buffer.AsSpan(0, byteCount));
        }

        private void SendInt(int n)
        {
            Span<byte> intBuffer = this.buffer.AsSpan(0, sizeof(int));
            BitConverter.TryWriteBytes(intBuffer, n);
            this.SendBytes(intBuffer);
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
