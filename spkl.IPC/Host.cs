using spkl.IPC.Messaging;
using System.IO;

namespace spkl.IPC
{
    public class Host
    {
        public string FilePath { get; private set; }

        public IClientHandler Handler { get; private set; }

        private MessageChannelHost MessageChannelHost { get; set; }

        private Host()
        {
        }

        public static Host Start(string filePath, IClientHandler handler)
        {
            File.Delete(filePath);

            Host host = new()
            {
                FilePath = filePath,
                Handler = handler
            };

            host.AcceptConnections();
            return host;
        }

        private void AcceptConnections()
        {
            this.MessageChannelHost = new MessageChannelHost(this.FilePath, this.HandleNewMessageChannel);
            this.MessageChannelHost.AcceptConnections();
        }

        private void HandleNewMessageChannel(MessageChannel channel)
        {
            ClientProperties properties = new ClientProperties();

            channel.Sender.SendReqArgs();
            properties.Arguments = channel.Receiver.ReceiveArgs();

            channel.Sender.SendReqCurrentDir();
            properties.CurrentDirectory = channel.Receiver.ReceiveCurrentDir();

            this.Handler.HandleCall(new ClientConnection(properties, channel));
        }

        public void Shutdown()
        {
            this.MessageChannelHost.Shutdown();
            File.Delete(this.FilePath);
        }
    }
}
