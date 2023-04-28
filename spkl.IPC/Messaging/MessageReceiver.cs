using System;
using System.Net.Sockets;
using System.Text;

namespace spkl.IPC.Messaging
{
    public class MessageReceiver
    {
        public MessageChannel Channel { get; }

        public Socket Socket { get; }

        private byte[] buffer;

        internal MessageReceiver(MessageChannel channel, Socket socket)
        {
            this.Channel = channel;
            this.Socket = socket;
            this.buffer = new byte[4];
        }

        public MessageType ReceiveMessage()
        {
            ReadOnlySpan<byte> result = this.ExpectBytesOrConnectionEnd(sizeof(MessageType));
            if (result.Length == 0)
            {
                return MessageType.ConnClosed;
            }

            return (MessageType)result[0];
        }

        public void ReceiveReqArgs()
        {
            if (this.ReceiveMessage() != MessageType.ReqArgs)
            {
                throw new Exception("Expected MessageType.ReqArgs"); // TODO is exception right? exception type?
            }
        }

        public void ReceiveReqCurrentDir()
        {
            if (this.ReceiveMessage() != MessageType.ReqCurrentDir)
            {
                throw new Exception("Expected MessageType.ReqCurrentDir"); // TODO is exception right? exception type?
            }
        }

        public string[] ReceiveArgs()
        {
            if (this.ReceiveMessage() != MessageType.Args)
            {
                throw new Exception("Expected MessageType.Args"); // TODO is exception right? exception type?
            }

            int nArgs = this.ExpectInt();
            string[] args = new string[nArgs];
            for (int i = 0; i < nArgs; i++)
            {
                args[i] = this.ExpectString();
            }

            return args;
        }

        public string ReceiveCurrentDir()
        {
            if (this.ReceiveMessage() != MessageType.CurrentDir)
            {
                throw new Exception("Expected MessageType.Args"); // TODO is exception right? exception type?
            }

            return this.ExpectString();
        }

        public string ExpectString()
        {
            int length = this.ExpectInt();
            ReadOnlySpan<byte> messageBytes = this.ExpectBytes(length);
            return Encoding.UTF8.GetString(messageBytes);
        }

        public int ExpectInt()
        {
            ReadOnlySpan<byte> n = this.ExpectBytes(sizeof(int));
            return BitConverter.ToInt32(n);
        }

        public ReadOnlySpan<byte> ExpectBytes(int expectedBytes)
        {
            ReadOnlySpan<byte> result = this.ExpectBytesOrConnectionEnd(expectedBytes);
            if (expectedBytes != 0 && result.Length == 0)
            {
                throw new Exception("Connection was closed.");
            }

            return result;
        }

        public ReadOnlySpan<byte> ExpectBytesOrConnectionEnd(int expectedBytes)
        {
            if (expectedBytes == 0)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            if (this.buffer.Length < expectedBytes)
            {
                this.buffer = new byte[expectedBytes];
            }

            int bytesReceived;
            int totalBytesReceived = 0;
            while (totalBytesReceived < expectedBytes)
            {
                bytesReceived = this.Socket.Receive(this.buffer, totalBytesReceived, expectedBytes - totalBytesReceived, SocketFlags.None);
                totalBytesReceived += bytesReceived;
                if (bytesReceived == 0)
                {
                    // Connection was closed
                    return ReadOnlySpan<byte>.Empty;
                }
            }

            return this.buffer.AsSpan(0, expectedBytes);
        }
    }
}
