// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Messaging;
using spkl.CLI.IPC.Services;
using System.Net.Sockets;

namespace spkl.CLI.IPC;

/// <summary>
/// The client.
/// </summary>
public class Client
{
    private MessageChannel Channel { get; }

    internal IHostConnectionHandler Handler { get; }

    private Client(MessageChannel channel, IHostConnectionHandler handler)
    {
        this.Channel = channel;
        this.Handler = handler;
    }

    /// <summary>
    /// Connects to a host using the specified <paramref name="transport"/> and <paramref name="handler"/>.
    /// This method blocks until the connection is closed.
    /// </summary>
    /// <exception cref="ConnectionException">The connection could not be established or the server closed the connection unexpectedly. See inner exception for details.</exception>
    public static void Attach(ITransport transport, IHostConnectionHandler handler)
    {
        MessageChannel channel;
        try
        {
            channel = ServiceProvider.MessageChannelFactory.CreateForOutgoing(transport);
        }
        catch (SocketException e)
        {
            throw new ConnectionException($"Could not connect. Reason: {e.Message}. Error code: {e.ErrorCode}.", e);
        }

        Client client = new(channel, handler);

        try
        {
            client.SendClientProperties();
            client.RunReceiveLoop();
        }
        catch (SocketException e)
        {
            throw new ConnectionException($"There was an unexpected connection error. Reason: {e.Message}", e);
        }
        finally
        {
            channel.Close();
        }
    }

    private void SendClientProperties()
    {
        this.Channel.Receiver.ReceiveReqArgs();
        this.Channel.Sender.SendArgs(this.Handler.Arguments);

        this.Channel.Receiver.ReceiveReqCurrentDir();
        this.Channel.Sender.SendCurrentDir(this.Handler.CurrentDirectory);

        this.Channel.Receiver.ReceiveReqProcessID();
        this.Channel.Sender.SendProcessID(this.Handler is IHostConnectionHandler2 connectionHandler2 ? connectionHandler2.ProcessID : -1);
    }

    private void RunReceiveLoop()
    {
        bool receivedExit = false;
        MessageType messageType;
        while ((messageType = this.Channel.Receiver.ReceiveMessage()) != MessageType.ConnClosed)
        {
            this.Receive(ref messageType, ref receivedExit);
        }

        if (!receivedExit)
        {
            throw new ConnectionException("The connection was closed without receiving the exit code.");
        }
    }

    private void Receive(ref MessageType messageType, ref bool receivedExit)
    {
        if (messageType == MessageType.OutStr)
        {
            string str = this.Channel.Receiver.ExpectString();
            this.Handler.HandleOutString(str);
        }
        else if (messageType == MessageType.ErrStr)
        {
            string str = this.Channel.Receiver.ExpectString();
            this.Handler.HandleErrorString(str);
        }
        else if (messageType == MessageType.Exit)
        {
            int exitCode = this.Channel.Receiver.ExpectInt();
            this.Handler.HandleExit(exitCode);
            receivedExit = true;
        }
        else
        {
            throw new ConnectionException($"Received unexpected message type '{messageType}' after the connection was established.");
        }
    }
}
