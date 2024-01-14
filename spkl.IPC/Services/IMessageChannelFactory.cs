using spkl.IPC.Messaging;
using System.Net.Sockets;

namespace spkl.IPC.Services;
public interface IMessageChannelFactory
{
    MessageChannel CreateForIncoming(Socket socket);

    MessageChannel CreateForOutgoing(ITransport transport);
}
