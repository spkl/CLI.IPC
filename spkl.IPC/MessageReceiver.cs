using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace spkl.IPC
{
    public class MessageReceiver
    {
        private readonly Socket socket;
        private byte[] buffer;

        internal MessageReceiver(Socket socket)
        {
            this.socket = socket;
            this.buffer = new byte[4];
        }

        public MessageType ReceiveMessage()
        {
            ReadOnlySpan<byte> result = this.ExpectBytesOrConnectionEnd(sizeof(MessageType));
            if (result.Length == 0)
            {
                return MessageType.ConnectionClosed;
            }

            return (MessageType)result[0];
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
                bytesReceived = socket.Receive(this.buffer, totalBytesReceived, expectedBytes - totalBytesReceived, SocketFlags.None);
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
