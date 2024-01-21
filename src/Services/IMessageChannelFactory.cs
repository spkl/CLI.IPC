// Copyright (c) Sebastian Fischer. All Rights Reserved.
// Licensed under the MIT License.

using spkl.CLI.IPC.Messaging;
using System.Net.Sockets;

namespace spkl.CLI.IPC.Services;
public interface IMessageChannelFactory
{
    MessageChannel CreateForIncoming(Socket socket);

    MessageChannel CreateForOutgoing(ITransport transport);
}
