// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using System.Net.Sockets;

namespace spkl.CLI.IPC.Messaging;

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
}
