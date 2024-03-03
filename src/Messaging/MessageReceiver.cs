// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Net.Sockets;
using System.Text;
#if NET6_0_OR_GREATER
using Bytes = System.ReadOnlySpan<byte>;
#else
using Bytes = System.ArraySegment<byte>;
#endif

namespace spkl.CLI.IPC.Messaging;

internal class MessageReceiver
{
    public Socket Socket { get; }

    private byte[] buffer;

    internal MessageReceiver(Socket socket)
    {
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
        Bytes result = this.ExpectBytesOrConnectionEnd(sizeof(MessageType));
        if (MessageReceiver.IsZeroBytes(result))
        {
            return MessageType.ConnClosed;
        }

        return (MessageType)MessageReceiver.GetFirstByte(result);
    }

    public void ReceiveReqArgs()
    {
        this.ExpectMessageType(MessageType.ReqArgs);
    }

    public void ReceiveReqCurrentDir()
    {
        this.ExpectMessageType(MessageType.ReqCurrentDir);
    }

    public void ReceiveReqProcessID()
    {
        this.ExpectMessageType(MessageType.ReqProcessID);
    }

    public string[] ReceiveArgs()
    {
        this.ExpectMessageType(MessageType.Args);

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
        this.ExpectMessageType(MessageType.CurrentDir);
        return this.ExpectString();
    }


    public int ReceiveProcessID()
    {
        this.ExpectMessageType(MessageType.ProcessID);
        return this.ExpectInt();
    }

    private void ExpectMessageType(MessageType expectedType)
    {
        MessageType messageType = this.ReceiveMessage();
        if (messageType != expectedType)
        {
            throw new ConnectionException($"Received unexpected message type '{messageType}' when trying to establish the connection. Expected {expectedType}.");
        }
    }

    public string ExpectString()
    {
        int length = this.ExpectInt();
        Bytes messageBytes = this.ExpectBytes(length);
#if NET6_0_OR_GREATER
        return Encoding.UTF8.GetString(messageBytes);
#else
        return Encoding.UTF8.GetString(messageBytes.Array, messageBytes.Offset, messageBytes.Count);
#endif
    }

    public int ExpectInt()
    {
        Bytes n = this.ExpectBytes(sizeof(int));
#if NET6_0_OR_GREATER
        return BitConverter.ToInt32(n);
#else
        return BitConverter.ToInt32(n.Array, n.Offset);
#endif
    }

    public Bytes ExpectBytes(int expectedBytes)
    {
        Bytes result = this.ExpectBytesOrConnectionEnd(expectedBytes);
        if (expectedBytes != 0 && MessageReceiver.IsZeroBytes(result))
        {
            throw new ConnectionException("The connection was closed unexpectedly.");
        }

        return result;
    }

    public Bytes ExpectBytesOrConnectionEnd(int expectedBytes)
    {
        if (expectedBytes == 0)
        {
            return MessageReceiver.GetZeroBytes();
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
                return MessageReceiver.GetZeroBytes();
            }
        }

        return this.GetBytesFromBuffer(0, expectedBytes);
    }

    private static Bytes GetZeroBytes()
    {
#if NET6_0_OR_GREATER
        return ReadOnlySpan<byte>.Empty;
#else
        return new ArraySegment<byte>(Array.Empty<byte>());
#endif
    }

    private static bool IsZeroBytes(Bytes bytes)
    {
#if NET6_0_OR_GREATER
        return bytes.Length == 0;
#else
        return bytes.Count == 0;
#endif
    }

    private static byte GetFirstByte(Bytes bytes)
    {
#if NET6_0_OR_GREATER
        return bytes[0];
#else
        return bytes.Array[bytes.Offset];
#endif
    }

    private Bytes GetBytesFromBuffer(int start, int length)
    {
#if NET6_0_OR_GREATER
        return this.buffer.AsSpan(start, length);
#else
        return new ArraySegment<byte>(this.buffer, start, length);
#endif
    }
}
