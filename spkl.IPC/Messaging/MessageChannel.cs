using System.Net.Sockets;

namespace spkl.IPC.Messaging;

public class MessageChannel
{
    public Socket Socket { get; }

    public MessageReceiver Receiver { get; }

    public MessageSender Sender { get; }

    internal MessageChannel(Socket socket)
    {
        this.Socket = socket;
        this.Receiver = new MessageReceiver(this, socket);
        this.Sender = new MessageSender(this, socket);
    }

    public void Close()
    {
        this.Socket.Close();
    }

    public static MessageChannel ConnectTo(ITransport transport)
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
