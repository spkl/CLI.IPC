using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace spkl.IPC.Messaging
{
    public class MessageChannelHost
    {
        public Socket Socket { get; }

        private Action<MessageChannel> handleNewConnection;

        private Action<Exception> handleListenerException;

        private Thread listenerThread;

        public MessageChannelHost(string filePath, Action<MessageChannel> handleNewConnection, Action<Exception> handleListenerException)
        {
            this.Socket = MessageChannel.GetSocket();
            this.Socket.Bind(MessageChannel.GetEndPoint(filePath));
            
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
                    Task.Factory.StartNew(() => this.handleNewConnection(messageChannel));
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
}
