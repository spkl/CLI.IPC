// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Messaging;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Services;

internal class MessageChannelFactory : IMessageChannelFactory
{
    public MessageChannel CreateForIncoming(Socket socket)
    {
        return new MessageChannel(socket);
    }

    public MessageChannel CreateForOutgoing(ITransport transport)
    {
        Socket? socket = null;
        try
        {
            socket = transport.Socket;
            socket.Connect(transport.EndPoint);
        }
        catch (SocketException)
        {
            socket?.Close();
            throw;
        }

        return new MessageChannel(socket);
    }
}
