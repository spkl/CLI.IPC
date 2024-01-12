using spkl.IPC.Messaging;
using System;
using System.Net.Sockets;

namespace spkl.IPC;

public class Host
{
    public ITransport Transport { get; }

    public IClientConnectionHandler Handler { get; }

    private MessageChannelHost? MessageChannelHost { get; set; }

    private Host(ITransport transport, IClientConnectionHandler handler)
    {
        this.Transport = transport;
        this.Handler = handler;
    }

    public static Host Start(ITransport transport, IClientConnectionHandler handler)
    {
        transport.BeforeHostStart();

        Host host = new(transport, handler);
        host.AcceptConnections();
        return host;
    }

    private void AcceptConnections()
    {
        this.MessageChannelHost = new MessageChannelHost(this.Transport, this.Handler.TaskFactory, this.HandleNewMessageChannel, this.HandleListenerException);
        this.MessageChannelHost.AcceptConnections();
    }

    private void HandleNewMessageChannel(MessageChannel channel)
    {
        ClientProperties properties;
        try
        {
            properties = this.ReceiveClientProperties(channel);
        }
        catch (SocketException e)
        {
            this.Handler.HandleListenerError(new ListenerError(e, false));
            return;
        }

        try
        {
            this.Handler.HandleCall(new ClientConnection(properties, channel));
        }
        catch
        {
            channel.Close();
            throw;
        }
    }

    private void HandleListenerException(Exception exception)
    {
        this.Handler.HandleListenerError(new ListenerError(exception, true));
    }

    private ClientProperties ReceiveClientProperties(MessageChannel channel)
    {
        ClientProperties properties = new ClientProperties();

        channel.Sender.SendReqArgs();
        properties.Arguments = channel.Receiver.ReceiveArgs();

        channel.Sender.SendReqCurrentDir();
        properties.CurrentDirectory = channel.Receiver.ReceiveCurrentDir();

        return properties;
    }

    public void Shutdown()
    {
        this.MessageChannelHost?.Shutdown();
        this.Transport.AfterHostShutdown();
    }
}
