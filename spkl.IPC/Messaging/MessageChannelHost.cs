using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Messaging;

public class MessageChannelHost
{
    public Socket Socket { get; }

    private readonly TaskFactory taskFactory;

    private readonly Action<MessageChannel> handleNewConnection;

    private readonly Action<Exception> handleListenerException;

    private Thread? listenerThread;

    public MessageChannelHost(ITransport transport, TaskFactory taskFactory, Action<MessageChannel> handleNewConnection, Action<Exception> handleListenerException)
    {
        this.Socket = transport.Socket;
        this.Socket.Bind(transport.EndPoint);
        
        this.taskFactory = taskFactory;
        this.handleNewConnection = handleNewConnection;
        this.handleListenerException = handleListenerException;
    }

    public void AcceptConnections()
    {
        this.Socket.Listen();
        this.listenerThread = new Thread(new ThreadStart(this.Accept));
        this.listenerThread.Name = $"{nameof(MessageChannelHost)} listener for {this.Socket.LocalEndPoint}";
        this.listenerThread.Start();
    }

    public void Shutdown()
    {
        this.Socket.Close();
    }

    private void Accept()
    {
        try
        {
            while (true)
            {
                Socket incoming = this.Socket.Accept();
                MessageChannel messageChannel = new MessageChannel(incoming);
                // TODO does this need to do some kind of exception passing, because the exception in the task would be hidden?
                this.taskFactory.StartNew(() => this.handleNewConnection(messageChannel));
            }
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
        {
            // This host was shut down - allow thread to end.
        }
        catch (Exception e)
        {
            this.handleListenerException(e);
        }
        finally
        {
            this.Socket.Close();
        }
    }
}
