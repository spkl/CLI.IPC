// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Net.Sockets;
using System.Text;
#if NET6_0_OR_GREATER
using Bytes = System.Span<byte>;
#else
using Bytes = System.ArraySegment<byte>;
#endif

namespace spkl.CLI.IPC.Messaging;

internal class MessageSender
{
    public Socket Socket { get; }

    private byte[] buffer;

    internal MessageSender(Socket socket)
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

    public void SendReqArgs()
    {
        this.SendMessageType(MessageType.ReqArgs);
    }

    public void SendReqCurrentDir()
    {
        this.SendMessageType(MessageType.ReqCurrentDir);
    }

    public void SendReqProcessID()
    {
        this.SendMessageType(MessageType.ReqProcessID);
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

    public void SendProcessID(int processId)
    {
        this.SendMessageType(MessageType.ProcessID);
        this.SendInt(processId);
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
        Bytes messageTypeBuffer = this.GetBytesFromBuffer(0, 1);
#if NET6_0_OR_GREATER
        messageTypeBuffer[0] = (byte)type;
#else
        messageTypeBuffer.Array[messageTypeBuffer.Offset] = (byte)type;
#endif
        this.SendBytes(messageTypeBuffer);
    }

    private void SendString(string str)
    {
        int byteCount = Encoding.UTF8.GetByteCount(str);
        this.SendInt(byteCount);

        this.EnsureBufferSize(byteCount);
        Encoding.UTF8.GetBytes(str, 0, str.Length, this.buffer, 0);
        this.SendBytes(this.GetBytesFromBuffer(0, byteCount));
    }

    private void SendInt(int n)
    {
#if NET6_0_OR_GREATER
        Bytes intBuffer = this.GetBytesFromBuffer(0, sizeof(int));
        BitConverter.TryWriteBytes(intBuffer, n);
#else
        Bytes intBuffer = new ArraySegment<byte>(BitConverter.GetBytes(n));
#endif
        this.SendBytes(intBuffer);
    }

    private void SendBytes(Bytes buffer)
    {
        try
        {
            int bytesSent = 0;
            while (bytesSent < MessageSender.GetLength(buffer))
            {
#if NET6_0_OR_GREATER
                bytesSent += this.Socket.Send(buffer[bytesSent..]);
#else
                bytesSent += this.Socket.Send(buffer.Array, buffer.Offset + bytesSent, buffer.Count - bytesSent, SocketFlags.None);
#endif
            }
        }
        catch (SocketException e)
        {
            throw new ConnectionException($"There was an unexpected connection error. Reason: {e.Message}", e);
        }
    }

    private Bytes GetBytesFromBuffer(int start, int length)
    {
#if NET6_0_OR_GREATER
        return this.buffer.AsSpan(start, length);
#else
        return new ArraySegment<byte>(this.buffer, start, length);
#endif
    }

    private static int GetLength(Bytes bytes)
    {
#if NET6_0_OR_GREATER
        return bytes.Length;
#else
        return bytes.Count;
#endif
    }
}
