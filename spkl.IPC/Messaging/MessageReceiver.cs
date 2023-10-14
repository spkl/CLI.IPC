using System;
using System.Net.Sockets;
using System.Text;

namespace spkl.IPC.Messaging;

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

    private void EnsureBufferSize(int requiredSize)
    {
        if (this.buffer.Length < requiredSize)
        {
            this.buffer = new byte[requiredSize];
        }
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
        MessageType messageType = this.ReceiveMessage();
        if (messageType != MessageType.ReqArgs)
        {
            throw new ConnectionException($"Received unexpected message type '{messageType}' when trying to establish the connection. Expected {MessageType.ReqArgs}.");
        }
    }

    public void ReceiveReqCurrentDir()
    {
        MessageType messageType = this.ReceiveMessage();
        if (messageType != MessageType.ReqCurrentDir)
        {
            throw new ConnectionException($"Received unexpected message type '{messageType}' when trying to establish the connection. Expected {MessageType.ReqCurrentDir}.");
        }
    }

    public string[] ReceiveArgs()
    {
        MessageType messageType = this.ReceiveMessage();
        if (messageType != MessageType.Args)
        {
            throw new ConnectionException($"Received unexpected message type '{messageType}' when trying to establish the connection. Expected {MessageType.Args}.");
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
        MessageType messageType = this.ReceiveMessage();
        if (messageType != MessageType.CurrentDir)
        {
            throw new ConnectionException($"Received unexpected message type '{messageType}' when trying to establish the connection. Expected {MessageType.CurrentDir}.");
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
            throw new ConnectionException("The connection was closed unexpectedly.");
        }

        return result;
    }

    public ReadOnlySpan<byte> ExpectBytesOrConnectionEnd(int expectedBytes)
    {
        if (expectedBytes == 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        this.EnsureBufferSize(expectedBytes);

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
