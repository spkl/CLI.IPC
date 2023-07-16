using spkl.IPC.Internal;
using spkl.IPC.Messaging;
using System.IO;

namespace spkl.IPC
{
    public class ClientConnection
    {
        public ClientProperties Properties { get; }

        internal MessageChannel Channel { get; }

        public TextWriter Out { get; }

        public TextWriter Error { get; }

        public ClientConnection(ClientProperties properties, MessageChannel channel)
        {
            this.Properties = properties;
            this.Channel = channel;
            this.Out = new DelegateTextWriter(this.Channel.Sender.SendOutStr);
            this.Error = new DelegateTextWriter(this.Channel.Sender.SendErrStr);
        }

        public void Exit(int exitCode)
        {
            this.Channel.Sender.SendExit(exitCode); // TODO handle exceptions
            this.Channel.Close();
        }
    }
}
